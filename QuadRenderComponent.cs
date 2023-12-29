// Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;

namespace VertexTest;

public partial class QuadRenderComponent : IDisposable
{
    private VertexPositionColor[] vertexBuffer;
    private short[] indexBuffer;
    private GraphicsDevice graphicsDevice;
    private bool isStarted;
    private int vertexCount;
    private int indexCount;
    private int shapeCount;
    private Effect effect;

    public QuadRenderComponent(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        const int MaxVertexCount = 1024;
        const int MaxIndexCount = 1024 / 2;
        vertexBuffer = new VertexPositionColor[MaxVertexCount];
        indexBuffer = new short[MaxIndexCount];

        isStarted = false;
    }

    public void SetEffect(Effect effect) 
    {
        this.effect = effect;
    }

    public void Begin() 
    {
        if (isStarted) 
        {
            throw new System.Exception("Batch were already started.");
        }

        Vector2 center = new Vector2(320 * 0.5f, 180 * 0.5f);
        var view = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));
        var projection = Matrix.CreateOrthographic(center.X * 2, center.Y * 2, -0.5f, 1);

        effect.Parameters["view"].SetValue(view);
        effect.Parameters["projection"].SetValue(projection);

        isStarted = true;
    }

    public void End() 
    {
        FlushVertex();
    }

    private void CheckIfStarted() 
    {
        if (!isStarted) 
        {
            throw new System.Exception("Batch were never started");
        }
    }

    private void FlushIfNeeded(int vertexCount, int indexCount) 
    {
        if (vertexCount > vertexBuffer.Length || indexCount > indexBuffer.Length) 
        {
            throw new System.Exception("Maximum shape vertex count is: " + vertexBuffer.Length);
        }

        if (this.vertexCount + vertexCount > vertexBuffer.Length || 
            this.indexCount + indexCount > indexBuffer.Length)
        {
            FlushVertex();
        }
    }

    public void FlushVertex() 
    {
        if (shapeCount == 0) 
        {
            return;
        }
        CheckIfStarted();

        foreach (var pass in effect.CurrentTechnique.Passes) 
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, 
            vertexBuffer, 0, vertexCount, indexBuffer, 0, indexCount / 3);
        }
    }

    public void Reset() 
    {
        shapeCount = 0;
        vertexCount = 0;
        indexCount = 0;
        isStarted = false;
    }

    public void DrawVertexQuad(int x1, int y1, int x2, int y2, Color color) 
    {
        FlushIfNeeded(4, 6);

        indexBuffer[indexCount]       = (short)(0 + vertexCount);
        indexBuffer[indexCount + 1]   = (short)(1 + vertexCount);
        indexBuffer[indexCount + 2]   = (short)(2 + vertexCount);
        indexBuffer[indexCount + 3]   = (short)(1 + vertexCount);
        indexBuffer[indexCount + 4]   = (short)(2 + vertexCount);
        indexBuffer[indexCount + 5]   = (short)(3 + vertexCount);

        indexCount += 6;

        vertexBuffer[vertexCount]     = new VertexPositionColor(new Vector3(x1, y1, 0), color); 
        vertexBuffer[vertexCount + 1] = new VertexPositionColor(new Vector3(x1, y1, 2), color); 
        vertexBuffer[vertexCount + 2] = new VertexPositionColor(new Vector3(x2, y2, 1), color); 

        vertexBuffer[vertexCount + 3] = new VertexPositionColor(new Vector3(x2, y2, 3), color); 

        vertexCount += 4;
        shapeCount++;
    }

    public void Dispose()
    {
    }
}