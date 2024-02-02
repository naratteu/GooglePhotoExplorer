using CasCap.Common.Extensions;
using CasCap.Models;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;

public class Cache
{
    Cache() { }
    readonly Loader<LocalPhoto> lLoader = new() { Path = "./cache.l" };
    readonly Loader<GooglePhoto> gLoader = new() { Path = "./cache.g" };
    public static readonly Task<Cache> Inst = Task.Run(async () =>
    {
        Cache c = new();
        await Task.WhenAll(c.lLoader.Load(), c.gLoader.Load());
        return c;
    });
    public void ClearFault()
    {
        lLoader.ClearFault();
        gLoader.ClearFault();
    }

    class Loader<T> where T : class, ICache
    {
        public required string Path { get; init; }
        ActionBlock<T>? ab;
        public T Add { set => _ = ab!.Post(value); }
        public async Task Load()
        {
            await File.AppendAllLinesAsync(Path, []);
            await foreach (var line in File.ReadLinesAsync(Path))
                if (line.FromJSON<T>() is { } t)
                    dic[t.Key] = Task.FromResult(t);
            ab = new(t => File.AppendAllLinesAsync(Path, [t.ToJSON()]));
        }
        public readonly ConcurrentDictionary<string, Task<T>> dic = new();
        public void ClearFault()
        {
            foreach (var kv in dic.ToLookup(kv => kv.Value.IsFaulted)[true])
                _ = dic.TryRemove(kv);
        }
    }

    static readonly AverageHash avg = new();
    static readonly DifferenceHash dif = new();
    static readonly PerceptualHash per = new();

    static readonly MD5 md5 = MD5.Create();
    static string ComputeHash(Func<Stream> f)
    {
        using var r = f();
        return Convert.ToBase64String(md5.ComputeHash(r));
    }
    public Task<LocalPhoto> Get(FileInfo f, ref string? key) => Get(f.OpenRead, ref key);
    public Task<LocalPhoto> Get(Func<Stream> f, ref string? key) => lLoader.dic.GetOrAdd(key ??= ComputeHash(f), md5 => Task.Run(() =>
    {
        using var r = f();
        return lLoader.Add = new()
        {
            md5 = md5,
            ImageHash = new()
            {
                avg = hash(avg, r),
                dif = hash(dif, r),
                per = hash(per, r),
                preview = base64(r),
            },
            CachedDate = DateTime.Now,
        };
    }));

    static readonly HttpClient web = new();
    public Task<GooglePhoto> Get(MediaItem mi) => gLoader.dic.GetOrAdd(mi.id, id => Task.Run(async () =>
    {
        var get = (await web.GetAsync(mi.baseUrl)).Content;
        await get.LoadIntoBufferAsync();
        var r = await get.ReadAsStreamAsync(); //GetStreamAsync는 포지션 리셋 에러남
        return gLoader.Add = new()
        {
            id = id,
            ImageHash = new()
            {
                avg = hash(avg, r),
                dif = hash(dif, r),
                per = hash(per, r),
                preview = base64(r),
            },
            CachedDate = DateTime.Now,
        };
    }));
    static ulong hash(IImageHash h, Stream s) { s.Position = 0; return h.Hash(s); }
    static string base64(Stream s)
    {
        s.Position = 0;
        using var img = Image.FromStream(s);
        using var tum = img.GetThumbnailImage(120, 120, null, nint.Zero);
        using var ms = new MemoryStream();
        tum.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        return $"data:image/jpg;base64,{Convert.ToBase64String(ms.GetBuffer())}";
    }

    public interface ICache
    {
        string Key { get; }
        DateTime CachedDate { get; }
    }
    public interface ICachePhoto : ICache
    {
        ImageHash ImageHash { get; }
    }
    public class LocalPhoto : ICachePhoto
    {
        public required string md5;
        public required ImageHash ImageHash;
        public required DateTime CachedDate { get; init; }

        string ICache.Key => md5;
        ImageHash ICachePhoto.ImageHash => ImageHash;
    }

    public class GooglePhoto : ICachePhoto
    {
        public required string id;
        public required ImageHash ImageHash;
        public required DateTime CachedDate { get; init; }

        string ICache.Key => id;
        ImageHash ICachePhoto.ImageHash => ImageHash;
    }

    public struct ImageHash
    {
        public required ulong avg, dif, per;
        public required string preview;
    }
}