namespace WINForm2
{
    public class Edge
    {
        //Greater index vertex
        public int StartVertex { set; get; }
        //Less index vertex
        public int EndVertex { set; get; }
        public Edge(int vertexStart, int vertexEnd)
        {
            StartVertex = vertexStart;
            EndVertex = vertexEnd;
        }
    }
}
