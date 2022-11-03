using System.Collections;
using System.Collections.Generic;
using Inspector;
using UnityEngine;

public class Sandbox : MonoBehaviour
{
    [Find("/"), SerializeField] private GameObject rootGameObject;

    [Find("/root-game-object"), SerializeField]
    private GameObject rootChild;

    [Find, SerializeField] private GameObject target;

    [Find("/", name: nameof(rootGameObject)), SerializeField]
    private SpriteRenderer rootSpriteRenderer;

    [Find("/root-game-object", name: nameof(rootChild)), SerializeField]
    private SpriteRenderer rootChildSpriteRenderer;

    // [Find("elements"), SerializeField] private GameObject[] elements;
    [Find("elements"), SerializeField] private SpriteRenderer[] elements;
    [Find, SerializeField] private GameObject[] children;


    #region self

    [SerializeField, Find(isExperimental: true)]
    private GameObject self;

    [SerializeField, Find(path: ".", isExperimental: true)]
    private SpriteRenderer selfSpriteRenderer;

    [SerializeField, Find(isExperimental: true)]
    private BoxCollider2D selfBoxCollider;

    // not found component
    // [SerializeField, Find(isExperimental: true)]
    // private AudioSource selfAudioSource;

    // not component type
    // [SerializeField, Find(isExperimental: true)]
    // private AudioClip selfAudioClip;

    #endregion

    #region child

    [SerializeField, Find("./", isExperimental: true)]
    private GameObject child;

    [SerializeField, Find("./", nameof(child), isExperimental: true)]
    private SpriteRenderer childSpriteRenderer;

    [SerializeField, Find("child/a", isExperimental: true)]
    private GameObject a1;

    [SerializeField, Find("child/b", isExperimental: true)]
    private BoxCollider2D b1;

    [SerializeField, Find("child/b", "b1", isExperimental: true)]
    private GameObject b1GameObject;

    [SerializeField, Find("child/c/c1", isExperimental: true)]
    private GameObject c2;

    [SerializeField, Find("child/c/c1", "c2", isExperimental: true)]
    private BoxCollider2D c2Collider;

    [Find("/", isExperimental: true)] [SerializeField]
    private GameObject root;

    [SerializeField, Find("/", "root", isExperimental: true)]
    private AudioSource rootAudioSource;

    [SerializeField, Find("/root", isExperimental: true)]
    private GameObject rootA;

    [SerializeField, Find("/root/root-b", isExperimental: true)]
    private SpriteRenderer rootB1;

    [SerializeField, Find("/root/root-b", "root-b1", isExperimental: true)]
    private GameObject rootB1GameObject;

    [SerializeField, Find(isExperimental: true)]
    private GameObject[] childrenExp;

    [SerializeField, Find("./elements", isExperimental: true)]
    private GameObject[] gameObjectElements;

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