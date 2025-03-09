using UnityEngine;

public class SpriteTransparentableSorterRenderer : SpriteSorterRenderer
{
    public override void Sort(int order)
    {
        base.Sort(order);
    }
    private void SetTransperent(float alpha)
    {
        //var isTransperent = HasIntersection(input);

        for (var i = 0; i < renderers.Count; i++)
        {
            renderers[i].color = SetAlpha(renderers[i].color, alpha);
        }
    }
    private bool HasIntersection(Vector3 input)
    {
        if (input.y < transform.position.y) return false;
        if ((input - transform.position).sqrMagnitude > 100) return false;
        return true;
    }
    private Color SetAlpha(Color c,float alpha)
    {
        return new Color(c.r,c.g,c.b,alpha);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SetTransperent(.5f);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        SetTransperent(1);
    }
}