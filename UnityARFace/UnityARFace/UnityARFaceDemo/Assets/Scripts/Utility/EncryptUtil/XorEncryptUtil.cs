
using System.Text;

public static class XorEncryptUtil
{
    private static byte[] key = new byte[]
    {
        0xE6, 0x80, 0xAA, 0xE7, 0x89, 0xA9, 0xE7, 0xA7, 0x91, 0xE6, 0x8A, 0x80
    };

    public static byte[] Encrypt(byte[] buffer)
    {
        int len = key.Length;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(buffer[i] ^ key[i % len]);
        }
        return buffer;
    }

    public static string EncryptString(byte[] buffer)
    {
        buffer = Encrypt(buffer);
        return Encoding.UTF8.GetString(buffer);
    }

    public static byte[] Decrypt(byte[] buffer)
    {
        return Encrypt(buffer);
    }

    public static string DecryptString(byte[] buffer)
    {
        return DecryptString(buffer);
    }
}
