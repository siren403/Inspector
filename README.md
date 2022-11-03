# Inspector

```text
root
    root-a
    - SpriteRenderer
        root-a1
        - BoxCollider
    root-b
A
B
    origin
    - SpriteRenderer
    - BoxCollider
        child-a
            child-a1
            - SpriteRenderer
            - BoxCollider
        child-b
        child-c
        - SpriteRenderer
            child-c1
C
```
```csharp

// Self
// this.GetComponent<T>()와 같다
[Find(".")]
public SpriteRenderer selfSpriteRenderer;
[Find]
public BoxCollider selfBoxCollider;

// Children 
// - (path+name)으로 검색
// - name을 지정하기 않으면 toKebabCase(fieldName)으로 name지정

// 자식들 중에서 toKebabCase(fieldName)를 찾는다 
[Find("./")]
public GameObject childA;

[Find("./child-a")]
public GameObject childA1;

// [  path  ][ name ]
// ./child-a/child-a1의 GameObject에서 Component를 찾는다
// name의 경우 hyphen(-)이 없으면 kebab case로 변환
[Find("./child-a", nameof(childA1))]
public SpriteRenderer childA1SpriteRenderer;
[Find("./child-a", "child-a1")]
public BoxCollider childA1BoxCollider;

// Root
[Find("/")]
public GameObject root;
[Find("/root")]
public SpriteRenderer rootA;
[Find("/root/root-a")]
public BoxCollider rootA1;

```