using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ajustes de render y spawn para WebGL.
///
/// 1) En WebGL se salta el post-proceso por RenderTexture: la camara principal
///    dibuja el mundo directamente en pantalla y la camara HUD dibuja el brazo
///    encima con ClearFlags.Depth. La "Virtual Camera" (la que componia los
///    quads del post-proceso) se desactiva.
/// 2) En el Tutorial el jugador aparece a y=105.8 (por encima del techo) porque
///    el salto de intro omite el teletransporte al punto de inicio. Lo bajamos
///    al suelo con un raycast.
/// 3) Si el jugador cae fuera del nivel (OutOfBounds esta en y=-200) el juego lo
///    devuelve a StatsManager.spawnPos, que en el Tutorial es y=105.8 (posicion
///    inicial de la escena). Forzamos spawnPos al suelo del inicio y, ademas,
///    atrapamos al jugador si baja de y=-50 antes de que OutOfBounds actue.
/// 4) Al saltar la intro, el pozo del inicio (FirstRoom/Room/Pit (3)/Cube (1))
///    queda activo; su TeleportPlayer suma +70 en Y y creaba un bucle. Se apaga
///    su collider al colocar al jugador en el suelo (la intro original lo
///    desactiva al terminar).
/// 5) Las escenas que no tienen skybox propia usan la skybox por defecto de
///    Unity (Default-Skybox, el builtin fileID 10304). Como RenderSettings.skybox
///    nunca es null, la camara principal acababa con ClearFlags.Skybox y se veia
///    el cielo azul de Unity al salirse del nivel. En Prelude, Limbo y
///    Gluttony se quita esa skybox y el fondo se queda en negro. Lust (2-1..2-4)
///    y Cyber Grind tienen skybox propia y no se tocan.
/// </summary>
public class WebGLRenderFix : MonoBehaviour
{
    private static WebGLRenderFix instance;

    private static Vector3 floorSpawn;
    private static bool floorSpawnValid;

    private float nextCheck;

    private const float TutorialCeiling = 50f;
    private const float TutorialFloorLimit = -50f;

    /// <summary>
    /// Niveles a los que se les pone fondo negro (quitan la skybox por defecto
    /// de Unity). Son los que usan el builtin Default-Skybox en su
    /// RenderSettings, asi que al salirse del nivel se veia el cielo azul de
    /// Unity. Lust (2-1..2-4) y Cyber Grind tienen skybox propia: fuera.
    /// </summary>
    private static readonly string[] BlackBackgroundScenes =
    {
        // Prelude (acto 0)
        "Level 0-1", "Level 0-2", "Level 0-3", "Level 0-4", "Level 0-5", "Level 0-S",
        // Limbo (acto 1)
        "Level 1-1", "Level 1-2", "Level 1-3", "Level 1-4", "Level 1-S",
        // Lust secreta (2-S usa la skybox por defecto de Unity)
        "Level 2-S",
        // Gluttony (acto 1)
        "Level 3-1", "Level 3-2",
        // Greed (acto 2): 4-S y P-1 usan la skybox por defecto de Unity.
        // 4-1, 4-2 y 4-4 traen skybox propia; 4-3 ya viene sin skybox.
        "Level 4-S", "Level P-1"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (instance != null)
        {
            return;
        }
        GameObject go = new GameObject("WebGLRenderFix");
        Object.DontDestroyOnLoad(go);
        instance = go.AddComponent<WebGLRenderFix>();

        // Los haces de luz de Minos Prime (materiales MinosSearchLights, shaders
        // unlit additive) se atenuan con _OpacScale. Es un uniform SUELTO: los
        // properties numericos no existen en la etapa de vertex (el backend
        // GLES no declara el cbuffer de material), asi que lo fija el codigo,
        // igual que _ResY o _ColorPrecision. 0.1 = un 90% menos luz (muy transparentes).
        Shader.SetGlobalFloat("_OpacScale", 0.1f);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartDelayed();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void StartDelayed()
    {
        StartCoroutine(ApplyDelayed(0.1f));
        StartCoroutine(ApplyDelayed(0.75f));
        StartCoroutine(ApplyDelayed(2f));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySkybox();
        StartDelayed();
    }

    private IEnumerator ApplyDelayed(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        ApplySkybox();
        ApplyCameras();
        FixTutorialState();
    }

    /// <summary>
    /// Quita la skybox por defecto de Unity (el builtin Default-Skybox, fileID
    /// 10304) en los niveles de la lista y deja el fondo en negro. Sin esto,
    /// RenderSettings.skybox nunca es null, la camara principal limpia con
    /// ClearFlags.Skybox y al salirse del nivel se ve el cielo azul de Unity.
    /// Los niveles de la lista referencian ese builtin en su RenderSettings
    /// (comprobado en las 25 escenas), asi que no hay ningun cielo real que
    /// perder: Lust (2-1..2-4) y Cyber Grind tienen skybox propia y no estan
    /// en la lista.
    /// </summary>
    private static void ApplySkybox()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (System.Array.IndexOf(BlackBackgroundScenes, sceneName) < 0)
        {
            return;
        }

        Material sky = RenderSettings.skybox;
        if (sky == null)
        {
            return;
        }

        RenderSettings.skybox = null;
        Debug.Log("[WebGLRenderFix] Fondo negro en '" + sceneName + "' (se quita la skybox '" + sky.name + "').");

        foreach (Camera c in Resources.FindObjectsOfTypeAll<Camera>())
        {
            if (c.name == "Main Camera" && c.gameObject.scene.isLoaded)
            {
                c.backgroundColor = Color.black;
            }
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheck)
        {
            return;
        }
        nextCheck = Time.unscaledTime + 0.5f;

        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            return;
        }

        NewMovement player = Object.FindObjectOfType<NewMovement>();
        if (player == null)
        {
            return;
        }

        FixTutorialState();
        EnforceStartTriggers();
    }

    private static StatsManager GetStats()
    {
        StatsManager stats = MonoSingleton<StatsManager>.Instance;
        if (stats == null)
        {
            stats = Object.FindObjectOfType<StatsManager>();
        }
        return stats;
    }

    private static void SetSpawnPos(StatsManager stats, Vector3 pos)
    {
        if (stats == null)
        {
            return;
        }
        if ((stats.spawnPos - pos).sqrMagnitude < 0.01f)
        {
            return;
        }
        stats.spawnPos = pos;
    }

    private void Teleport(NewMovement player, Vector3 target)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = target;
            rb.WakeUp();
        }
        player.transform.position = target;
    }

    private static Vector3 ComputeFloorTarget(float x, float z, NewMovement player)
    {
        float bottomOffset = 1.5f;
        if (player != null)
        {
            Collider col = player.GetComponent<Collider>();
            if (col != null)
            {
                bottomOffset = player.transform.position.y - col.bounds.min.y;
                if (bottomOffset <= 0f || bottomOffset > 10f)
                {
                    bottomOffset = 1.5f;
                }
            }
        }

        Vector3 fallback = Vector3.zero;
        bool haveFallback = false;

        foreach (Vector2 off in FloorProbeOffsets)
        {
            float px = x + off.x;
            float pz = z + off.y;
            RaycastHit hit;
            if (!Physics.Raycast(new Vector3(px, 4.5f, pz), Vector3.down, out hit, 40f, ~0, QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            float targetY = hit.point.y + bottomOffset + 0.1f;
            Vector3 target = new Vector3(px, targetY, pz);

            if (OverlapsTeleportTrigger(target, player))
            {
                if (off.x == 0f && off.y == 0f)
                {
                    // Punto natural dentro de un trigger de teletransporte (el pit
                    // del inicio devuelve +70 al jugador y crea el bucle). Se apaga.
                    DisableTeleportTriggersAt(target, player);
                }
                if (OverlapsTeleportTrigger(target, player))
                {
                    if (!haveFallback)
                    {
                        fallback = target;
                        haveFallback = true;
                    }
                    continue;
                }
            }

            return target;
        }

        if (haveFallback)
        {
            return fallback;
        }

        return new Vector3(x, 0f, z);
    }

    private static readonly Vector2[] FloorProbeOffsets = new Vector2[]
    {
        new Vector2(0f, 0f),
        new Vector2(0f, -4f),
        new Vector2(0f, 4f),
        new Vector2(0f, -8f),
        new Vector2(0f, 8f),
        new Vector2(0f, -12f),
        new Vector2(0f, 12f),
        new Vector2(4f, -6f),
        new Vector2(-4f, -6f),
        new Vector2(4f, 6f),
        new Vector2(-4f, 6f),
    };

    private static Collider[] OverlapPlayerCapsule(Vector3 target, NewMovement player)
    {
        CapsuleCollider cc = player != null ? player.GetComponent<CapsuleCollider>() : null;
        Vector3 p0;
        Vector3 p1;
        float radius;
        if (cc != null)
        {
            Quaternion rot = player.transform.rotation;
            Vector3 center = target + rot * cc.center;
            float half = Mathf.Max(cc.height * 0.5f - cc.radius, 0.01f);
            radius = cc.radius + 0.05f;
            p0 = center - Vector3.up * half;
            p1 = center + Vector3.up * half;
        }
        else
        {
            radius = 0.55f;
            p0 = target + Vector3.up * 0.2f;
            p1 = target + Vector3.up * 2.2f;
        }
        return Physics.OverlapCapsule(p0, p1, radius, ~0, QueryTriggerInteraction.Collide);
    }

    private static bool OverlapsTeleportTrigger(Vector3 target, NewMovement player)
    {
        foreach (Collider c in OverlapPlayerCapsule(target, player))
        {
            if (c == null || !c.enabled)
            {
                continue;
            }
            if (c.GetComponentInParent<TeleportPlayer>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private static void DisableTeleportTriggersAt(Vector3 target, NewMovement player)
    {
        foreach (Collider c in OverlapPlayerCapsule(target, player))
        {
            if (c == null)
            {
                continue;
            }
            TeleportPlayer tp = c.GetComponentInParent<TeleportPlayer>();
            if (tp == null)
            {
                continue;
            }
            // OJO: OnTriggerEnter se sigue recibiendo aunque el MonoBehaviour este
            // desactivado; hay que apagar el COLLIDER para cortar el trigger.
            if (c.enabled)
            {
                c.enabled = false;
            }
            if (tp.enabled)
            {
                tp.enabled = false;
            }
        }
    }

    /// <summary>
    /// Red de seguridad: si algo reactiva el pit del inicio (o instancia un clon),
    /// se vuelve a apagar. Solo afecta a la zona del inicio (los pits del final,
    /// que son FinalPit, estan a y=-111 y quedan fuera).
    /// </summary>
    private static void EnforceStartTriggers()
    {
        foreach (TeleportPlayer tp in Resources.FindObjectsOfTypeAll<TeleportPlayer>())
        {
            if (tp == null || !tp.gameObject.scene.isLoaded)
            {
                continue;
            }
            Vector3 p = tp.transform.position;
            if (Mathf.Abs(p.x) > 25f || p.y < -30f || p.y > 30f || p.z < -100f || p.z > -30f)
            {
                continue;
            }
            Collider c = tp.GetComponent<Collider>();
            if (c != null && c.enabled)
            {
                c.enabled = false;
            }
            if (tp.enabled)
            {
                tp.enabled = false;
            }
        }
    }

    private void FixTutorialState()
    {
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            return;
        }

        // Al saltar la intro, Over() desactiva sus objetos antes de que el
        // IntroTextController llegue a poner inIntro = false. OptionsManager
        // ignora el boton de pausa mientras inIntro sea true, asi que lo
        // limpiamos aqui (la intro ya no existe en el port WebGL).
        OptionsManager options = MonoSingleton<OptionsManager>.Instance;
        if (options != null && options.inIntro)
        {
            options.inIntro = false;
        }

        NewMovement player = Object.FindObjectOfType<NewMovement>();
        if (player == null)
        {
            return;
        }

        Vector3 pos = player.transform.position;
        StatsManager stats = GetStats();

        if (pos.y > TutorialCeiling)
        {
            // Arriba del techo: bajamos al suelo y fijamos ahi el respawn.
            Vector3 target = ComputeFloorTarget(pos.x, pos.z, player);
            if (!floorSpawnValid || pos.y > 90f)
            {
                floorSpawn = target;
                floorSpawnValid = true;
            }
            Teleport(player, target);
            SetSpawnPos(stats, floorSpawn);
            return;
        }

        if (pos.y < TutorialFloorLimit)
        {
            // En la zona del FinalPit (final del Tutorial, z>100) el juego hace su
            // secuencia de caida al pozo; no intervenir o se rompe el final.
            if (pos.z > 100f)
            {
                return;
            }
            // Debajo del nivel: devolvemos al inicio antes de que el juego lo
            // mande al spawnPos alto.
            Vector3 target = floorSpawnValid ? floorSpawn : ComputeFloorTarget(pos.x, pos.z, player);
            Teleport(player, target);
            SetSpawnPos(stats, target);
            return;
        }

        if (floorSpawnValid)
        {
            SetSpawnPos(stats, floorSpawn);
        }
        else
        {
            floorSpawn = pos;
            floorSpawnValid = true;
            SetSpawnPos(stats, floorSpawn);
        }
    }

    private static void ApplyCameras()
    {
        Camera[] cameras = Resources.FindObjectsOfTypeAll<Camera>()
            .Where(c => c.gameObject.scene.isLoaded)
            .ToArray();

        foreach (Camera c in cameras)
        {
            if (c.name == "Main Camera")
            {
                c.enabled = true;
                c.targetTexture = null;
                // Los niveles con skybox (Lust) traen la camara con ClearFlags.Skybox.
                // Forzar SolidColor dejaba el cielo en negro; solo se fuerza cuando
                // la escena no tiene skybox (niveles interiores).
                c.clearFlags = RenderSettings.skybox != null
                    ? CameraClearFlags.Skybox
                    : CameraClearFlags.SolidColor;
                c.depth = -1f;
            }
            else if (c.name == "HUD Camera")
            {
                c.enabled = true;
                c.targetTexture = null;
                c.clearFlags = CameraClearFlags.Depth;
                c.depth = 0f;
            }
            else if (c.name == "Virtual Camera" || c.name == "Shop Camera")
            {
                c.enabled = false;
            }
        }

        // El handler de post-proceso asigna targetTexture a las camaras; en
        // WebGL lo desactivamos para que el mundo vaya directo a pantalla.
        foreach (ULTRAKILL_PostProcessHandler handler in Resources.FindObjectsOfTypeAll<ULTRAKILL_PostProcessHandler>())
        {
            if (handler != null && handler.gameObject.scene.isLoaded)
            {
                handler.enabled = false;
            }
        }

        // GREED usa PostProcessV2_Handler: se registra en Camera.onPreRender
        // (delegado estatico, no basta con desactivar el componente) y asigna
        // mainCam.targetTexture, dejando la pantalla en negro si sigue activo.
        foreach (PostProcessV2_Handler handlerV2 in Resources.FindObjectsOfTypeAll<PostProcessV2_Handler>())
        {
            if (handlerV2 != null && handlerV2.gameObject.scene.isLoaded)
            {
                handlerV2.enabled = false;
            }
        }
        Camera.onPreRender = null;
        Camera.onPostRender = null;

        // El OutOfBoundsTargetSetter del Tutorial pone el respawnTarget de los
        // DeathZone en el spawn alto.
        foreach (OutOfBoundsTargetSetter setter in Resources.FindObjectsOfTypeAll<OutOfBoundsTargetSetter>())
        {
            if (setter != null && setter.gameObject.scene.isLoaded)
            {
                setter.enabled = false;
            }
        }

        // La red de seguridad de y<-50 ya cubre caerse del mapa en el Tutorial.
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            foreach (OutOfBounds oob in Resources.FindObjectsOfTypeAll<OutOfBounds>())
            {
                if (oob != null && oob.gameObject.scene.isLoaded && oob.enabled)
                {
                    oob.enabled = false;
                }
            }
        }
    }
}
