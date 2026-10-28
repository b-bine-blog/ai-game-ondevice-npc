// 모바일용 가상 조이스틱 입력

using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform handle;
    private RectTransform background;
    private float radius;

    [HideInInspector] public Vector2 inputVector;

    void Start()
    {
        background = GetComponent<RectTransform>();
        radius = background.sizeDelta.x / 2;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, null, out pos))
        {
            // 핸들이 배경 원 밖으로 나가지 않게 제한
            pos = Vector2.ClampMagnitude(pos, radius);
            handle.localPosition = pos;

            // 0~1 범위의 입력값 계산
            inputVector = pos / radius;
        }
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.localPosition = Vector2.zero;
        inputVector = Vector2.zero;
    }
}
