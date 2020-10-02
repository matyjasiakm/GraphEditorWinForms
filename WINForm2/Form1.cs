using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.IO;
using System.Resources;
using System.Threading;
using System.Reflection;

namespace WINForm2
{
    public partial class Form1 : Form
    {
        int vertexLength = 30;
        int vertexWidth = 3;
        bool isVertexMoving = false;
        int vertexPosition_X = -1;
        int vertexPosition_Y = -1;

        List<Vertex> verticesList;
        List<Edge> edgesList;
        Vertex selectedVertex;
        ResourceManager resourceManager;
        Color color;
        public Form1()
        {
            InitializeComponent();
            edgesList = new List<Edge>();
            verticesList = new List<Vertex>();
            this.KeyPreview = true;
            DoubleBuffered = true;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pl-PL");
            resourceManager = new ResourceManager("WINForm2.Form1", Assembly.GetExecutingAssembly());
            loadButton.Select();
            selectedVertex = null;
            color = Color.Black;
        }
        /// <summary>
        /// Select vertex, add or remove edge and moving active vertex
        /// </summary>
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (CanAddVertexOnPosition(verticesList, e.X, e.Y, vertexLength))
                {
                    verticesList.Add(new Vertex(e.X, e.Y, color));
                }
                else
                {
                    if (selectedVertex != null)
                    {
                        int startVertexIndex = 0, endVertexIndex = 0;
                        GetClikedVertex(verticesList, e.X, e.Y, vertexLength, out Vertex clickedVertex);
                        for (int vertexIndex = verticesList.Count - 1; vertexIndex >= 0; vertexIndex--)
                        {
                            if (verticesList[vertexIndex] == selectedVertex)
                                startVertexIndex = vertexIndex + 1;
                            if (verticesList[vertexIndex] == clickedVertex)
                                endVertexIndex = vertexIndex + 1;
                        }
                        if (startVertexIndex != endVertexIndex)
                        {
                            int existedEdgeInEdgeList = -1;
                            foreach (var edge in edgesList)
                            {
                                if (edge.StartVertex == Math.Max(startVertexIndex, endVertexIndex) && edge.EndVertex == Math.Min(startVertexIndex, endVertexIndex))
                                {
                                    existedEdgeInEdgeList = edgesList.IndexOf(edge);
                                }
                            }
                            if (existedEdgeInEdgeList == -1)
                                edgesList.Add(new Edge(Math.Max(startVertexIndex, endVertexIndex), Math.Min(startVertexIndex, endVertexIndex)));
                            else
                                edgesList.RemoveAt(existedEdgeInEdgeList);
                        }
                    }
                }
                DrawGraph();
            }

            if (e.Button == MouseButtons.Right)
            {
                GetClikedVertex(verticesList, e.X, e.Y, vertexLength, out selectedVertex);

                if (selectedVertex != null)
                {
                    removeVertexButton.Enabled = true;
                    pictureBox2.BackColor = selectedVertex.Color;
                }
                else
                {
                    removeVertexButton.Enabled = false;
                }

                DrawGraph();
            }

            if (e.Button == MouseButtons.Middle)
            {
                if (selectedVertex != null)
                {
                    isVertexMoving = true;
                    vertexPosition_Y = e.Y;
                    vertexPosition_X = e.X;
                }
            }
        }
        /// <summary>
        /// Redraw picture after picture box size changed
        /// </summary>
        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            DrawGraph();
        }
        /// <summary>
        /// Change cursor style when mouse over picture box
        /// </summary>
        /// <param name="sender"></param>
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox.Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Change vertex color
        /// </summary>
        private void vertexColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog myColorDialog = new ColorDialog();

            if (myColorDialog.ShowDialog() == DialogResult.OK)
            {
                color = myColorDialog.Color;
                if (selectedVertex != null)
                {
                    selectedVertex.Color = myColorDialog.Color;
                    DrawGraph();
                }
                pictureBox2.BackColor = color;
            }
        }
        /// <summary>
        /// Check if can add new vertex
        /// </summary>
        /// <param name="vertexList">Vertex list</param>
        /// <param name="x">New vertex x position</param>
        /// <param name="y">New vertex y position</param>
        /// <param name="radius">New vertex radius</param>
        /// <returns>True if vertex can be added; otherwise false</returns>
        bool CanAddVertexOnPosition(List<Vertex> vertexList, int x, int y, int radius)
        {
            foreach (var vertex in vertexList)
            {
                if ((x - vertex.X) * (x - vertex.X) + (y - vertex.Y) * (y - vertex.Y) < radius * radius)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Search for clicked vertex
        /// </summary>
        /// <param name="verticesList">Vertices list</param>
        /// <param name="x">Clicked point x coordinate</param>
        /// <param name="y">Clicked point y coordinate</param>
        /// <param name="radius">Vertex radius</param>
        /// <param name="vertex">Clicked vertex or null if does not exist</param>
        /// <returns>True if there is clicked vertex; otherwise false</returns>
        bool GetClikedVertex(List<Vertex> verticesList, float x, float y, int radius, out Vertex vertex)
        {
            vertex = null;
            foreach (var v in verticesList)
            {
                if ((x - v.X) * (x - v.X) + (y - v.Y) * (y - v.Y) < radius * radius)
                {
                    if (vertex != null)
                    {
                        if (Math.Sqrt((x - v.X) * (x - v.X) + (y - v.Y) * (y - v.Y)) < Math.Sqrt((x - vertex.X) * (x - vertex.X) + (y - vertex.Y) * (y - vertex.Y)))
                            vertex = v;
                    }
                    else
                    {
                        vertex = v;
                    }
                }
            }
            return vertex != null ? true : false;
        }
        /// <summary>
        /// Load graph from file
        /// </summary>  
        private void loadButton_Click(object sender, EventArgs e)
        {
            bool isFileValid = true;

            OpenFileDialog newFileDialog = new OpenFileDialog();
            newFileDialog.Filter = "graph files (*.graph)|*.graph";

            if (newFileDialog.ShowDialog() == DialogResult.OK)
            {
                List<Vertex> readVertices = new List<Vertex>();
                List<Edge> readEdges = new List<Edge>();
                using (StreamReader reader = new StreamReader(newFileDialog.FileName))
                {
                    try
                    {
                        while (!reader.EndOfStream)
                        {
                            string[] readLine = reader.ReadLine().Split(',');
                            if (readLine.Length == 3)
                                readVertices.Add(new Vertex(Int32.Parse(readLine[0]), Int32.Parse(readLine[1]), ColorTranslator.FromHtml(readLine[2])));
                            else if (readLine.Length == 2)
                            {
                                if (Int32.Parse(readLine[0]) <= 0 || Int32.Parse(readLine[0]) > readVertices.Count || Int32.Parse(readLine[1]) <= 0 || Int32.Parse(readLine[1]) > readVertices.Count)
                                    throw new Exception();
                                readEdges.Add(new Edge(Int32.Parse(readLine[0]), Int32.Parse(readLine[1])));
                            }
                            else
                                throw new Exception();
                        }
                    }
                    catch
                    {
                        isFileValid = false;
                        MessageBox.Show(resourceManager.GetString("msgOpenWrong"));
                    }
                }
                if (isFileValid)
                {
                    verticesList = readVertices;
                    edgesList = readEdges;
                    DrawGraph();
                    MessageBox.Show(resourceManager.GetString("msgBoxOpen"));
                }
            }
        }
        /// <summary>
        /// Save graph to file
        /// </summary>
        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog newFileDialog = new SaveFileDialog();
            newFileDialog.Filter = "graph files (*.graph)|*.graph";
            if (newFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(newFileDialog.FileName))
                {
                    foreach (var vertex in verticesList)
                    {
                        writer.WriteLine(vertex.X.ToString() + "," + vertex.Y.ToString() + "," + vertex.Color.ToArgb());
                    }
                    foreach (var elem in edgesList)
                    {
                        writer.WriteLine(elem.StartVertex.ToString() + "," + elem.EndVertex.ToString());
                    }
                    MessageBox.Show(resourceManager.GetString("msgBoxSave"));
                }
            }
        }
        /// <summary>
        /// Clear graph
        /// </summary>
        private void clearButton_Click(object sender, EventArgs e)
        {
            verticesList.Clear();
            edgesList.Clear();
            removeVertexButton.Enabled = false;
            DrawGraph();
        }
        /// <summary>
        /// Delete vertex
        /// </summary>
        private void removeVertexButton_Click(object sender, EventArgs e)
        {
            int selectedVertexIndex = verticesList.IndexOf(selectedVertex) + 1;
            verticesList.Remove(selectedVertex);
            edgesList.RemoveAll(edge => edge.StartVertex == selectedVertexIndex || edge.EndVertex == selectedVertexIndex);

            foreach (var edge in edgesList)
            {
                if (edge.StartVertex > selectedVertexIndex)
                    edge.StartVertex--;
                if (edge.EndVertex > selectedVertexIndex)
                    edge.EndVertex--;
            }

            selectedVertex = null;
            removeVertexButton.Enabled = false;
            DrawGraph();
        }
        /// <summary>
        /// Draws graph
        /// </summary>
        void DrawGraph()
        {
            pictureBox.Refresh();
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);

            Graphics g = Graphics.FromImage(pictureBox.Image);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            Pen pen = new Pen(color, vertexWidth);

            StringFormat stringFormat = new StringFormat();
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Alignment = StringAlignment.Center;

            int vertexIndex = 1;

            Font drawFont = new Font("Arial", 12);
            pen.Color = Color.Black;

            foreach (var edge in edgesList)
            {
                g.DrawLine(pen, verticesList[edge.StartVertex - 1].X, verticesList[edge.StartVertex - 1].Y, verticesList[edge.EndVertex - 1].X, verticesList[edge.EndVertex - 1].Y);
            }

            foreach (var vertex in verticesList)
            {
                SolidBrush burshText = new SolidBrush(vertex.Color);
                SolidBrush brushElipse = new SolidBrush(Color.White);
                pen.Color = vertex.Color;
                g.FillEllipse(brushElipse, vertex.X - vertexLength / 2, vertex.Y - vertexLength / 2, vertexLength, vertexLength);

                if (vertex == selectedVertex)
                {
                    Pen p = new Pen(selectedVertex.Color, vertexWidth);
                    p.DashPattern = new float[] { 2.0F, 2.0F };
                    g.DrawEllipse(p, selectedVertex.X - vertexLength / 2, selectedVertex.Y - vertexLength / 2, vertexLength, vertexLength);
                    g.DrawString(vertexIndex.ToString(), Font, burshText, vertex.X, vertex.Y, stringFormat);
                }
                else
                {
                    g.DrawEllipse(pen, vertex.X - vertexLength / 2, vertex.Y - vertexLength / 2, vertexLength, vertexLength);
                    g.DrawString(vertexIndex.ToString(), Font, burshText, vertex.X, vertex.Y, stringFormat);
                }
                vertexIndex++;
            }
            g.Dispose();
            pen.Dispose();
            stringFormat.Dispose();
        }
        /// <summary>
        /// Moving selected vertex
        /// </summary>
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isVertexMoving == true)
            {
                selectedVertex.X -= (vertexPosition_X - e.X);
                selectedVertex.Y -= (vertexPosition_Y - e.Y);

                vertexPosition_Y = e.Y;
                vertexPosition_X = e.X;

                DrawGraph();
            }
        }
        /// <summary>
        /// Stop moving selected vertex
        /// </summary>
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isVertexMoving = false;

                if (selectedVertex != null)
                {
                    if (selectedVertex.X < 0)
                        selectedVertex.X = 0;
                    if (selectedVertex.Y < 0)
                        selectedVertex.Y = 0;
                    if (selectedVertex.Y > pictureBox.ClientSize.Height)
                        selectedVertex.Y = pictureBox.ClientSize.Height;
                    if (selectedVertex.X > pictureBox.ClientSize.Width)
                        selectedVertex.X = pictureBox.ClientSize.Width;
                }
                DrawGraph();
            }
        }
        /// <summary>
        /// Delete selected vertex with Del key
        /// </summary>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && selectedVertex != null)
            {
                int selectedVertexIndex = verticesList.IndexOf(selectedVertex) + 1;
                verticesList.Remove(selectedVertex);
                edgesList.RemoveAll(x => x.StartVertex == selectedVertexIndex || x.EndVertex == selectedVertexIndex);

                foreach (var edge in edgesList)
                {
                    if (edge.StartVertex > selectedVertexIndex)
                        edge.StartVertex--;
                    if (edge.EndVertex > selectedVertexIndex)
                        edge.EndVertex--;
                }

                selectedVertex = null;
                removeVertexButton.Enabled = false;
                DrawGraph();
            }
        }
        /// <summary>
        /// Set pl language
        /// </summary>
        private void plLanguageButton_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pl-PL");
            //save instance data
            Vertex selectedVertexTemp = selectedVertex;
            int top = Top;
            int left = Left;
            int width = Width;
            int height = Height;
            Controls.Clear();

            InitializeComponent();

            //load saved instance
            this.KeyPreview = true;
            DoubleBuffered = true;
            loadButton.Select();
            Focus();
            Top = top;
            Left = left;
            Width = width;
            Height = height;
            selectedVertex = selectedVertexTemp;
            if (selectedVertex != null) removeVertexButton.Enabled = true;

            DrawGraph();
        }
        /// <summary>
        /// Set eng language
        /// </summary>
        private void enLanguageButton_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            //save instance data
            Vertex selectedVertexTemp = selectedVertex;
            int top = Top;
            int left = Left;
            int width = Width;
            int height = Height;
            Controls.Clear();

            InitializeComponent();

            //load saved instance
            this.KeyPreview = true;
            DoubleBuffered = true;
            Focus();
            selectedVertex = selectedVertexTemp;
            loadButton.Select();
            Top = top;
            Left = left;
            Width = width;
            Height = height;
            if (selectedVertex != null) removeVertexButton.Enabled = true;

            DrawGraph();
        }
    }
}
