using UnityEngine;
using UnityEngine.EventSystems;

namespace CrossingTheWorld.TCCMenu
{
    // Ao passar o mouse, uso o mesmo destaque que as setas do teclado.
    public sealed class MenuPointer : MonoBehaviour, IPointerEnterHandler
    {
        public TravelMenu menu;
        public int index;
        public bool country;
        public void OnPointerEnter(PointerEventData eventData) { menu.Select(index, country); }
        public void Click() { menu.ClickOption(index, country); }
    }
}
