using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

namespace basirua
{
    public struct BrightestPoint
    {
        public Vector2 position;
        public float brightness;
        public int taken;
    }

    public struct BrightnessCount
    {
        public Vector2 positionStart;
        public Vector2 positionEnd;
        public int brightnessCount;
    }

    public class BrightestFinder
    {
        private ComputeShader shader;
        private ComputeBuffer buffer;
        private ComputeBuffer takenBuffer;

        private BrightestPoint[] brightestPoints = new BrightestPoint[20];
        private Vector2[] takenData = new Vector2[20];

        public BrightestFinder()
        {
            shader = Resources.Load<ComputeShader>("Shaders/BrightestCompute");
            if (shader == null)
                Debug.LogError("Failed to load <BrightestCompute> Compute Shader");

            InitializeBuffers();
        }

        private void InitializeBuffers()
        {
            for (int i = 0; i < 20; i++)
            {
                brightestPoints[i] = new BrightestPoint() { brightness = -50, position = new Vector2(-900, -900), taken = 0 };
                takenData[i] = new Vector2(999, 999);
            }

            buffer = new ComputeBuffer(20, sizeof(float) * 3 + sizeof(int));
            buffer.SetData(brightestPoints);

            takenBuffer = new ComputeBuffer(20, sizeof(float) * 2);
            takenBuffer.SetData(takenData);

            shader.SetBuffer(0, "takenBuffer", takenBuffer);
            shader.SetBuffer(1, "takenBuffer", takenBuffer);
            shader.SetBuffer(0, "_BrightestPoint", buffer);
            shader.SetBuffer(1, "_BrightestPoint", buffer);
        }

        public Texture2D ProcessTexture(Texture2D sourceTexture, int newWidth, int newHeight, Vector2 center)
        {
            Texture2D resizedTexture = new Texture2D(newWidth, newHeight);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int sourceX = Mathf.RoundToInt(center.x) + x - newWidth / 2;
                    int sourceY = Mathf.RoundToInt(center.y) + y - newHeight / 2;

                    // If the source pixel is within bounds, copy it to the new texture
                    if (sourceX >= 0 && sourceX < sourceTexture.width && sourceY >= 0 && sourceY < sourceTexture.height)
                    {
                        resizedTexture.SetPixel(x, y, sourceTexture.GetPixel(sourceX, sourceY));
                    }
                    else
                    {
                        // If the source pixel is outside bounds, set the pixel to black
                        resizedTexture.SetPixel(x, y, Color.black);
                    }
                }
            }

            resizedTexture.Apply();
            return resizedTexture;
        }


        public Texture2D DrawDebugCircle(Texture2D texture, Vector2 drawLoc, float size)
        {
            Color[] pixels = texture.GetPixels();   
            float height = texture.height;
            float width = texture.width;

            for (int i = 0; i < pixels.Length; i++)
            {
                int pX = (int)(i % width );
                int pY = (int)(i / height);
                
                if ((new Vector2(pX, pY) - drawLoc).magnitude < size)
                {
                    pixels[i] = Color.blue*0.5f;
                }
            }
            
            texture.SetPixels(pixels);

            return texture;
        }

        public float RadialProfileOfLight(Texture2D texture, Vector2 galaxyMid, float stepMultiplier, int steps)
        {
            float petrosianR = 0;
            float[] intensityByRadius = new float[steps];
            
            Color[] pixels = texture.GetPixels();   
            float height = texture.height;
            float width = texture.width;

            for (int i = 0; i < pixels.Length; i++)
            {
                int pX = (int)(i % width );
                int pY = (int)(i / height);

                for (int s = 1; s < steps; s++)
                {
                    float radius = s * stepMultiplier;
                    if ((new Vector2(pX, pY) - galaxyMid).magnitude < radius)
                    {
                        float pixelLuminosity = pixels[i].r;
                        float circleRadius = Mathf.PI * radius * radius;
                        
                        intensityByRadius[s-1] += (pixelLuminosity) / (circleRadius);
                    }
                }
            }

            petrosianR = (intensityByRadius.Sum() * 0.5f) * 100f;

            return Mathf.Max(petrosianR, 100);
        }
        
        public Texture2D RadialProfileOfLightDebug(Texture2D texture, Vector2 galaxyMid, float stepMultiplier, int steps, [CanBeNull] string objName)
        {
            //float petrosianR = 0;
            float[] intensityByRadius = new float[steps];
            
            Color[] pixels = texture.GetPixels();   
            float height = texture.height;
            float width = texture.width;

            for (int i = 0; i < pixels.Length; i++)
            {
                int pX = (int)(i % width );
                int pY = (int)(i / height);
                
                for (int s = steps-1; s > 0; s--)
                {
                    float radius = s * stepMultiplier;
                    if ((new Vector2(pX, pY) - galaxyMid).magnitude < radius)
                    {
                        float pixelLuminosity = pixels[i].r;
                        float circleRadius = Mathf.PI * radius * radius;
                        
                        intensityByRadius[s-1] += (pixelLuminosity) / (circleRadius);
                        
                        pixels[i] = Color.green*0.1f * s;
                    }
                }
            }

            float petrosianR = (intensityByRadius.Sum() * 0.5f) * 100f;
            Debug.Log($"{objName} - PR: {petrosianR}");

            texture.SetPixels(pixels);
            return texture;
        }

        public Vector2 FindBrightestAreaCenter(Texture2D textureToScan, float brightnessThreshold, float minimumRadius = 11)
        {
            int width = textureToScan.width;
            int height = textureToScan.height;
            Color[] colorPixels = textureToScan.GetPixels();

            Vector2 brightestCenter = Vector2.zero;
            float brightestBrightness = 0f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = y * width + x;
                    Color pixelColor = colorPixels[pixelIndex];
                    float brightness = (pixelColor.r + pixelColor.g + pixelColor.b) / 3f; // Calculate brightness as an average of RGB channels

                    if (brightness > brightestBrightness && brightness >= brightnessThreshold)
                    {
                        // Check the distance from the current center to the previous brightest center
                        Vector2 currentCenter = new Vector2(x, y);
                        float distance = Vector2.Distance(currentCenter, brightestCenter);

                        if (distance >= minimumRadius)
                        {
                            brightestBrightness = brightness;
                            brightestCenter = currentCenter;
                        }
                    }
                }
            }

            return brightestCenter;
        }

    }
}
