namespace TPToolkitLib.Mesh.Structs
{
    public struct ObjTriangle
    {
        public int[] P0;
        public int[] P1;
        public int[] P2;

        public ObjTriangle()
        {
            P0 = [1, 1, 1];
            P1 = [1, 1, 1];
            P2 = [1, 1, 1];
        }

        public override string ToString()
        {
            return $"{P0[0]}/{P0[1]}/{P0[2]} {P1[0]}/{P1[1]}/{P1[2]} {P2[0]}/{P2[1]}/{P2[2]}";
        }
    }
}
