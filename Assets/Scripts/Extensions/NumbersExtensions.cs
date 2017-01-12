

public static class NumberTypeExtensions {

	public static float Normalized(this float thisFloat, float min = -1f, float max = 1f) {
		return (thisFloat - min) / (max - min);
	}

	public static float DeNormalized(this float thisFloat, float min = -1f, float max = 1f) {
		return (thisFloat * (max - min) - max);
	}
}
