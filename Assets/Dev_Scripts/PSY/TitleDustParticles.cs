using UnityEngine;

public class TitleDustParticles : MonoBehaviour
{
    private ParticleSystem ps;

    [Header("Particle")]
    [SerializeField] private int maxParticles = 120;
    [SerializeField] private float minSize = 0.04f;
    [SerializeField] private float maxSize = 0.08f;

    [Header("Lifetime")]
    [SerializeField] private float minLifetime = 4f;
    [SerializeField] private float maxLifetime = 8f;

    [Header("Emission")]
    [SerializeField] private float emissionRate = 20f;

    [Header("Area")]
    [SerializeField] private Vector3 area = new Vector3(5f, 3f, 2f);

    [Header("Movement")]
    [SerializeField] private float noiseStrength = 0.7f;
    [SerializeField] private float noiseFrequency = 0.2f;
    [SerializeField] private float noiseScrollSpeed = 0.15f;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        SetupParticleSystem();
    }

    private void SetupParticleSystem()
    {
        // ========================================
        // Main
        // ========================================

        var main = ps.main;

        main.loop = true;
        main.playOnAwake = true;

        main.maxParticles = maxParticles;

        // 입자가 화면에 머무는 시간
        main.startLifetime = new ParticleSystem.MinMaxCurve(
            minLifetime,
            maxLifetime
        );

        // 한 방향으로 이동하지 않도록 거의 0
        main.startSpeed = new ParticleSystem.MinMaxCurve(
            0f,
            0.02f
        );

        // 작은 먼지
        main.startSize = new ParticleSystem.MinMaxCurve(
            minSize,
            maxSize
        );

        // 흰색 ~ 연노랑
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 1f),
            new Color(1f, 0.85f, 0.45f, 1f)
        );

        // 중력 없음
        main.gravityModifier = 0f;


        // ========================================
        // Emission
        // ========================================

        var emission = ps.emission;

        emission.enabled = true;
        emission.rateOverTime = emissionRate;


        // ========================================
        // Shape
        // ========================================

        var shape = ps.shape;

        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;

        // 파티클이 생성되는 공간
        shape.scale = area;


        // ========================================
        // Noise
        // ========================================

        var noise = ps.noise;

        noise.enabled = true;

        // 불규칙하게 떠다니는 정도
        noise.strength = noiseStrength;

        // 움직임의 변화 속도
        noise.frequency = noiseFrequency;

        // Noise 패턴 이동 속도
        noise.scrollSpeed = noiseScrollSpeed;

        // 움직임을 부드럽게
        noise.damping = true;

        noise.octaveCount = 2;


        // ========================================
        // Color Over Lifetime
        // ========================================

        var color = ps.colorOverLifetime;

        color.enabled = true;

        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
                // 흰색으로 시작
                new GradientColorKey(
                    Color.white,
                    0f
                ),

                // 중간에 살짝 따뜻한 노랑
                new GradientColorKey(
                    new Color(1f, 0.9f, 0.55f),
                    0.5f
                ),

                // 마지막에 다시 흰색
                new GradientColorKey(
                    Color.white,
                    1f
                )
            },

            new GradientAlphaKey[]
            {
                // 생성 직후 투명
                new GradientAlphaKey(
                    0f,
                    0f
                ),

                // 천천히 나타남
                new GradientAlphaKey(
                    1f,
                    0.15f
                ),

                // 대부분의 시간 동안 보임
                new GradientAlphaKey(
                    0.9f,
                    0.7f
                ),

                // 자연스럽게 사라짐
                new GradientAlphaKey(
                    0f,
                    1f
                )
            }
        );

        color.color = new ParticleSystem.MinMaxGradient(
            gradient
        );


        // ========================================
        // Size Over Lifetime
        // ========================================

        var size = ps.sizeOverLifetime;

        size.enabled = true;

        AnimationCurve sizeCurve = new AnimationCurve();

        // 생성
        sizeCurve.AddKey(0f, 0.15f);

        // 조금 커짐
        sizeCurve.AddKey(0.15f, 0.8f);

        // 중간
        sizeCurve.AddKey(0.5f, 1f);

        // 다시 작아짐
        sizeCurve.AddKey(0.8f, 0.8f);

        // 사라질 때 작아짐
        sizeCurve.AddKey(1f, 0.1f);

        size.size = new ParticleSystem.MinMaxCurve(
            1f,
            sizeCurve
        );


        // ========================================
        // Renderer
        // ========================================

        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        renderer.renderMode =
            ParticleSystemRenderMode.Billboard;

        renderer.alignment =
            ParticleSystemRenderSpace.View;


        // ========================================
        // Play
        // ========================================

        ps.Play();
    }
}