OVERVIEW
--------

    This is an example integration of Soft Mask and TextMesh Pro. This package doesn't
    provide an 'official' integration - it's just a sample of how it could be 
    implemented. The example isn't tested thoughtfully and it may not work in some 
    (many?) cases. Use this solution at your own risk.

    The packages SoftMask and TextMesh Pro should be imported into the project in 
    order the sample to work.

HOW DOES IT WORK?
-----------------

    Soft Mask uses IMaterialModifier to dynamically override material's shader of
    the child elements. Unfortunately, the current version of TextMesh Pro ignores
    IMaterialModifier completely.
    
    To overcome this limitation a class that is derived from TextMeshProUGUI was
    written. It's very simple class that only overrides the `materialForRendering'
    property. Its implementation is taken from standard Unity's Graphic component
    (which is available on Bitbucket).
    
    All that means that to use SoftMask with TMPro you have to use this script
    instead of the original TextMeshProUGUI. If you have sources of TextMeshPro you
    could also try to patch it but I'm not sure what consequences this modification
    would have.

    The remaining implementation is pretty straightforward and accords to Soft Mask
    documentation: one of TMPro shaders (in this sample Mobile shader is chosen)
    was copied and instructions for SoftMask support were added. This approach should
    work for any other TMPro UI shader too.

HOW TO USE?
-----------

    1. Add a specialized SDF-shader with SoftMask support. An example could be found
       at Resources/LiberaionSans SDF - Soft Mask.shader. All the places where changes
       were made are marked with `// Soft Mask' comment.

    2. Add a material that uses this shader:
       Resources/LiberationSans SDF - Soft Mask.mat.

    3. Use the TextMeshProUGUISoftMask script instead of original TextMeshProUGUI.
       Assign the created material to this script and set up other properties as
       you need.
