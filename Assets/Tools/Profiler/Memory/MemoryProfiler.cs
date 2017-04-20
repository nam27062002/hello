using UnityEngine;
public class MemoryProfiler
{ 
    private MemorySample CurrentMemorySample { get; set; }

    public void TakeASample()
    {
        if (CurrentMemorySample == null)
        {
            CurrentMemorySample = new MemorySample();
        }
        else
        {
            CurrentMemorySample.Reset();
        }

        HideFlags hideFlagMask = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
        HideFlags hideFlagMask1 = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontUnloadUnusedAsset;

        //
        // Textures
        //
        Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();
        int count = textures.Length;
        Texture t;
        for (int i = 0; i < count; i++)
        {
            t = textures[i];

            // Internal stuff (editor) is not considered
            if (t.hideFlags == HideFlags.HideAndDontSave || t.hideFlags == hideFlagMask || t.hideFlags == hideFlagMask1)
                continue;

            CurrentMemorySample.AddTexture(t);            
        }
        
        Mesh[] meshes = Resources.FindObjectsOfTypeAll<Mesh>();
        count = meshes.Length;
        Mesh m;
        for (int i = 0; i < count; i++)
        {
            m = meshes[i];
            if (m.hideFlags == HideFlags.HideAndDontSave || m.hideFlags == hideFlagMask || m.hideFlags == hideFlagMask1)
                continue;

            CurrentMemorySample.AddMesh(m);            
        }        
        
        CurrentMemorySample.CalculateStats();
        Debug.Log(CurrentMemorySample.ToXML().OuterXml);
    }	
}
