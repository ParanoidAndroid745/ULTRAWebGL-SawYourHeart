using UnityEditor;
using UnityEngine;

public static class ShaderCheck
{
    public static void Check()
    {
        Shader s = Shader.Find("UI/Default_UK");
        if (s == null)
        {
            Debug.LogError("SHADERCHECK: UI/Default_UK NOT FOUND");
            return;
        }
        Debug.Log("SHADERCHECK: found=" + s.name + " supported=" + s.isSupported +
                  " hasError=" + ShaderUtil.ShaderHasError(s));
        ShaderMessage[] messages = ShaderUtil.GetShaderMessages(s);
        Debug.Log("SHADERCHECK: messages=" + messages.Length);
        foreach (ShaderMessage m in messages)
        {
            Debug.Log("SHADERCHECK: " + m.platform + " " + m.severity + " " + m.message +
                      " (" + m.file + ":" + m.line + ")");
        }
    }
}
