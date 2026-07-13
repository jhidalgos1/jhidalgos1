using System.Security.Cryptography;
namespace JH.QueryStudio.Api.Security;
public interface ICredentialProtector{string Protect(string plain);string Unprotect(string cipher);}
public sealed class AesCredentialProtector(IConfiguration cfg):ICredentialProtector{
 byte[] Key{get{var f=cfg["Security:KeyFile"]??"Data/jh-query-studio.key";Directory.CreateDirectory(Path.GetDirectoryName(f)!);if(!File.Exists(f))File.WriteAllBytes(f,RandomNumberGenerator.GetBytes(32));return File.ReadAllBytes(f);}}
 public string Protect(string plain){using var aes=Aes.Create();aes.Key=Key;aes.GenerateIV();using var enc=aes.CreateEncryptor();var bytes=enc.TransformFinalBlock(System.Text.Encoding.UTF8.GetBytes(plain),0,plain.Length);return Convert.ToBase64String(aes.IV.Concat(bytes).ToArray());}
 public string Unprotect(string cipher){var all=Convert.FromBase64String(cipher);using var aes=Aes.Create();aes.Key=Key;aes.IV=all[..16];using var dec=aes.CreateDecryptor();return System.Text.Encoding.UTF8.GetString(dec.TransformFinalBlock(all,16,all.Length-16));}
}
