
using UnityEngine;

public interface ITextureHandler
{
    public void SaveToPath(Texture2D texture);

    public void SaveToPng(byte[] bytes, string path);

    public Texture2D LoadFromPath(int texH, int texW);
}