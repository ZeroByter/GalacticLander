using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

class GetPixelResponse {
    public Color32 pixel = new Color32();

    public GetPixelResponse(Color32 pixel) {
        this.pixel = pixel;
    }
}

class GetNearestEdgeResponse {
    public Vector2 pixel = new Vector2();

    public GetNearestEdgeResponse(Vector2 pixel) {
        this.pixel = pixel;
    }
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class AutoTileColliderMaker : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polyCollider;

    private GetPixelResponse GetPixel(Color32[] pixels, int width, int x, int y) {
        int index = (y * width) + x;

        if (index >= 0 && index < pixels.Length) {
            return new GetPixelResponse(pixels[index]);
        } else {
            return null;
        }
    }

    private GetNearestEdgeResponse GetNearestEdge(List<Vector2> edgePoints, Vector2 current) {
        for(int x = (int)current.x - 1; x <= current.x + 1; x++) {
            for (int y = (int)current.y - 1; y <= current.y + 1; y++) {
                if (x == 0 && y == 0) continue; //x == 0 and y == 0 is simply the current pixel, boring

                if (edgePoints.Contains(new Vector2(x, y))) return new GetNearestEdgeResponse(new Vector2(x, y));
            }
        }

        return null;
    }

    private int CountNeighbourPixels(Sprite sprite, Vector2 pixel) {
        int counter = 0;

        Color32[] pixels = sprite.texture.GetPixels32();

        for (int x = (int)pixel.x - 1; x <= pixel.x + 1; x++) {
            for (int y = (int)pixel.y - 1; y <= pixel.y + 1; y++) {
                if (x == 0 && y == 0) continue; //x == 0 and y == 0 is simply the current pixel, boring
                if (x < sprite.rect.x || x > sprite.rect.x + sprite.rect.width - 1) continue;
                if (y <= sprite.rect.y || y > sprite.rect.y + sprite.rect.height - 1) continue;

                GetPixelResponse color = GetPixel(pixels, sprite.texture.width, x, y);
                if (color != null) {
                    if (color.pixel.a > 0) counter++;
                }
            }
        }

        return counter;
    }

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polyCollider = GetComponent<PolygonCollider2D>();
    }

    private void Start() {
        if (spriteRenderer.sprite == null) return;

        //set offset
        polyCollider.offset = new Vector2(-0.5f, -7.5f);

        //variables and stuff, boring stuff
        Sprite sprite = spriteRenderer.sprite;
        Texture2D texture = sprite.texture;
        Color32[] pixels = texture.GetPixels32();

        List<Vector2> edgePoints = new List<Vector2>();

        //First we identify all edges
        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                if (x < sprite.rect.x || x > sprite.rect.x + sprite.rect.width - 1) continue;
                if (y < sprite.rect.y || y > sprite.rect.y + sprite.rect.height - 1) continue;

                GetPixelResponse currentPixel = GetPixel(pixels, texture.width, x, y);
                GetPixelResponse pixelBelow = GetPixel(pixels, texture.width, x, y + 1);
                GetPixelResponse pixelAbove = GetPixel(pixels, texture.width, x, y - 1);
                GetPixelResponse pixelLeft = GetPixel(pixels, texture.width, x - 1, y);
                GetPixelResponse pixelRight = GetPixel(pixels, texture.width, x + 1, y);

                if (
                    (pixelBelow != null && currentPixel.pixel.a == 0 && pixelBelow.pixel.a > 0) ||
                    (pixelAbove != null && currentPixel.pixel.a == 0 && pixelAbove.pixel.a > 0) ||
                    (pixelLeft != null && currentPixel.pixel.a == 0 && pixelLeft.pixel.a > 0) ||
                    (pixelRight != null && currentPixel.pixel.a == 0 && pixelRight.pixel.a > 0) ||
                    (x == sprite.rect.x && currentPixel.pixel.a > 0) ||
                    (x == sprite.rect.x + sprite.rect.width - 1 && currentPixel.pixel.a > 0) ||
                    (y == sprite.rect.y && currentPixel.pixel.a > 0) ||
                    (y == sprite.rect.y + sprite.rect.height - 1 && currentPixel.pixel.a > 0)) {
                        edgePoints.Add(new Vector2(x, y));
                }
            }
        }

        //apply debug texture
        //foreach (Vector2 edge in edgePoints) texture.SetPixel((int)edge.x, (int)edge.y, Color.white);
        //texture.Apply();

        List<Vector2> colliderPoints = new List<Vector2>();

        Vector2 lastEdgePoint = edgePoints[0];
        colliderPoints.Add(new Vector2(lastEdgePoint.x / sprite.rect.width - sprite.rect.x / sprite.rect.width, lastEdgePoint.y / sprite.rect.height - ((sprite.rect.y + sprite.rect.height) - texture.height) / sprite.rect.height));
        while (edgePoints.Count > 0) {
            edgePoints.Remove(lastEdgePoint);

            GetNearestEdgeResponse nearestEdgePointResponse = GetNearestEdge(edgePoints, lastEdgePoint);
            if (nearestEdgePointResponse != null) {
                Vector2 newEdgePoint = nearestEdgePointResponse.pixel;

                if(CountNeighbourPixels(sprite, lastEdgePoint) == 4 || (newEdgePoint.x != lastEdgePoint.x && newEdgePoint.y != lastEdgePoint.y)) { //if we are at a corner or the pixel positions are completely different
                    colliderPoints.Add(new Vector2(lastEdgePoint.x / sprite.rect.width - sprite.rect.x / sprite.rect.width, lastEdgePoint.y / sprite.rect.height - ((sprite.rect.y + sprite.rect.height) - texture.height) / sprite.rect.height));
                }
                
                lastEdgePoint = newEdgePoint;
            } else {
                break;
            }
        }

        polyCollider.points = colliderPoints.ToArray();
    }

}
