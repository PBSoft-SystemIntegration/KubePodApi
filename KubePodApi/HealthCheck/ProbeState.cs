namespace KubePodApi.HealthCheck;

// Kun til øvelsen: værdierne nulstilles, når containerens proces genstarter.
public static class ProbeState
{
    public static volatile bool Ready = true;
    public static volatile bool Alive = true;
}
