using UnityEngine;

public enum Direction { N, NE, E, SE, S, SW, W, NW, T, B }

[CreateAssetMenu(fileName = "SpriteSet", menuName = "Scriptable Objects/SpriteSet")]
public class SpriteSet : ScriptableObject
{
    [System.Serializable]
    public class DirecitonalFrames
    {
        public Direction dir;
        public Sprite[] frames;

        public DirecitonalFrames(Direction direction)
        {
            dir = direction;
            frames = new Sprite[0];
        }
    }

    public float defaultFps = 10f;
    public DirecitonalFrames[] views = new DirecitonalFrames[10]
    {
        new(Direction.N),
        new(Direction.NE),
        new(Direction.E),
        new(Direction.SE),
        new(Direction.S),
        new(Direction.SW),
        new(Direction.W),
        new(Direction.NW),
        new(Direction.T),
        new(Direction.B)
    };

    public Sprite GetFrame(Direction dir, int frameIdx)
    {
        var df = System.Array.Find(views, d => d.dir == dir);
        if (df == null || df.frames == null || df.frames.Length == 0) return null;
        int i = Mathf.Clamp(frameIdx % df.frames.Length, 0, df.frames.Length - 1);

        return df.frames[i];
    }
}
