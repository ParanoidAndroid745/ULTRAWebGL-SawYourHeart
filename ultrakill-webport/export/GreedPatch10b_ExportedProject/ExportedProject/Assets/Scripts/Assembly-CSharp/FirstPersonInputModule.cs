using UnityEngine;
using UnityEngine.EventSystems;

public class FirstPersonInputModule : StandaloneInputModule
{
	protected override MouseState GetMousePointerEventData(int id)
	{
#if UNITY_WEBGL
		// En WebGL, tocar Cursor.lockState hace que el navegador SUELTE el
		// bloqueo del raton y no lo vuelve a conceder sin un gesto del
		// usuario (la camara se queda sin poder girar 360 grados y Unity
		// ademas cree que sigue bloqueado, asi que nunca lo reintenta).
		// El juego ya suelta el raton por su cuenta cuando abre un menu
		// (pausa, tienda, salir), asi que aqui no hay que tocar nada.
		return base.GetMousePointerEventData(id);
#else
		CursorLockMode lockState = Cursor.lockState;
		Cursor.lockState = CursorLockMode.None;
		MouseState mousePointerEventData = base.GetMousePointerEventData(id);
		Cursor.lockState = lockState;
		return mousePointerEventData;
#endif
	}

	protected override void ProcessMove(PointerEventData pointerEvent)
	{
#if UNITY_WEBGL
		base.ProcessMove(pointerEvent);
#else
		CursorLockMode lockState = Cursor.lockState;
		Cursor.lockState = CursorLockMode.None;
		base.ProcessMove(pointerEvent);
		Cursor.lockState = lockState;
#endif
	}

	protected override void ProcessDrag(PointerEventData pointerEvent)
	{
#if UNITY_WEBGL
		base.ProcessDrag(pointerEvent);
#else
		CursorLockMode lockState = Cursor.lockState;
		Cursor.lockState = CursorLockMode.None;
		base.ProcessDrag(pointerEvent);
		Cursor.lockState = lockState;
#endif
	}
}
