using Inspector;
using UnityEngine;

public class Sandbox : MonoBehaviour
{
    [Find("/"), SerializeField] private GameObject rootGameObject;

    [Find("/root-game-object"), SerializeField]
    private GameObject rootChild;

    [Find("./"), SerializeField] private GameObject target;

    [Find("/", name: nameof(rootGameObject)), SerializeField]
    private SpriteRenderer rootSpriteRenderer;

    [Find("/root-game-object", name: nameof(rootChild)), SerializeField]
    private SpriteRenderer rootChildSpriteRenderer;

    [Find("elements"), SerializeField] private SpriteRenderer[] elements;
    [Find, SerializeField] private GameObject[] children;


    #region self

    [SerializeField, Find()] private GameObject self;

    [SerializeField, Find(path: ".")] private SpriteRenderer selfSpriteRenderer;

    [SerializeField, Find()] private BoxCollider2D selfBoxCollider;

    // not found component
    // [SerializeField, Find()]
    // private AudioSource selfAudioSource;

    // not component type
    // [SerializeField, Find()]
    // private AudioClip selfAudioClip;

    #endregion

    #region child

    [SerializeField, Find("./")] private GameObject child;

    [SerializeField, Find("./", nameof(child))]
    private SpriteRenderer childSpriteRenderer;

    [SerializeField, Find("child/a")] private GameObject a1;

    [SerializeField, Find("child/b")] private BoxCollider2D b1;

    [SerializeField, Find("child/b", "b1")]
    private GameObject b1GameObject;

    [SerializeField, Find("child/c/c1")] private GameObject c2;

    [SerializeField, Find("child/c/c1", "c2")]
    private BoxCollider2D c2Collider;

    #endregion

    #region root

    [Find("/")] [SerializeField] private GameObject root;

    [SerializeField, Find("/", "root")] private AudioSource rootAudioSource;

    [SerializeField, Find("/root")] private GameObject rootA;

    [SerializeField, Find("/root/root-b")] private SpriteRenderer rootB1;

    [SerializeField, Find("/root/root-b", "root-b1")]
    private GameObject rootB1GameObject;

    #endregion

    #region IList

    [SerializeField, Find()] private GameObject[] childrenExp;

    [SerializeField, Find("./elements")] private GameObject[] gameObjectElements;

    #endregion


    [Button]
    public void ResetExperimental()
    {
        self = null;
        selfSpriteRenderer = null;
        selfBoxCollider = null;

        child = null;
        childSpriteRenderer = null;
        a1 = null;
        b1 = null;
        b1GameObject = null;
        c2 = null;
        c2Collider = null;
    }

    #region from root

    #endregion
}