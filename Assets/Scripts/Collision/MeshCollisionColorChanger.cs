using UnityEngine;

public class MeshCollisionColorChanger : MonoBehaviour
{
    private Renderer objectRenderer; // 用于访问 Mesh 的材质

    void Start()
    {
        // 获取 Mesh Renderer 组件
        objectRenderer = GetComponent<Renderer>();

        // 确保对象有材质，否则抛出警告
        if (objectRenderer == null)
        {
            Debug.LogWarning("No Renderer found on the object. Please add a Mesh Renderer.");
        }
    }

    // 当发生碰撞时调用
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision detected with {collision.gameObject.name}");

        // 检查是否有 Renderer，并更改颜色
        if (objectRenderer != null)
        {
            objectRenderer.material.color = GetRandomColor();
        }
    }

    // 随机生成一个颜色
    private Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }
}
