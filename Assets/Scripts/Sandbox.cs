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
}