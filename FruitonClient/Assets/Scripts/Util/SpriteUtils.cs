using UnityEngine;

namespace Util
{
    /// <summary>
    /// Contains sprite related helper methods.
    /// </summary>
    public static class SpriteUtils
    {
        public static Sprite TextureToSprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}