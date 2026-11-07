using Unity.Collections;

namespace JobFind
{
    public interface IDigManager
    {
        public void ResetMark();
        public void MarkTile(int mark);
        public void ApplyMark(bool add);
    }
}