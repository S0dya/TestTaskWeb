using UnityEngine;

public static class Utils
{
    
    public static Sprite ConvertToSprite(this Texture2D texture, Vector2? pivot = null)
    {
        return Sprite.Create(
            texture, 
            new Rect(0, 0, texture.width, texture.height),
            pivot ?? new Vector2(0.5f, 0.5f));
    }
}
