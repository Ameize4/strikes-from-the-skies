using DG.Tweening;
using UnityEngine;

public class TweenTest : MonoBehaviour
{
    public Vector3 positionTarget;
    public Vector3 rotationTarget;

    private Vector3 positionInit;
    private Vector3 rotationInit;

    public float duration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        positionInit = transform.localPosition;
        rotationInit = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var s = DOTween.Sequence();
            s.Append(DOTween.To(
                () => transform.rotation, 
                x => transform.rotation = x, 
                rotationTarget, 
                duration).SetEase(Ease.OutQuad))
                .Join(DOTween.To(
                () => transform.localPosition, 
                x => transform.localPosition = x, 
                positionTarget, 
                duration).SetEase(Ease.OutQuad))
                .Append(DOTween.To(
                () => transform.rotation, 
                x => transform.rotation = x, 
                rotationInit, 
                duration).SetEase(Ease.InQuad))
                .Join(DOTween.To(
                () => transform.localPosition, 
                x => transform.localPosition = x, 
                positionInit, 
                duration).SetEase(Ease.InQuad));
        }
    }
}
