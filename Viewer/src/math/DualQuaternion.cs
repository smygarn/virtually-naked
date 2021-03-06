using ProtoBuf;
using SharpDX;

[ProtoContract(UseProtoMembersOnly = true)]
public struct DualQuaternion {
	[ProtoMember(1)]
	private Quaternion real;

	[ProtoMember(2)]
	private Quaternion dual;

	public Quaternion Real => real;
	public Quaternion Dual => dual;
	
	public DualQuaternion(Quaternion real, Quaternion dual) {
		this.real = real;
		this.dual = dual;
	}

	public static readonly DualQuaternion Identity = new DualQuaternion(Quaternion.Identity, Quaternion.Zero);

	private static readonly Vector3 Epsilon = 1e-5f * Vector3.One;

	public static DualQuaternion FromMatrix(Matrix matrix) {
		Vector3 translation = new Vector3(matrix.M41, matrix.M42, matrix.M43);

		var rotationMatrix = new Matrix3x3(
			matrix.M11, matrix.M12, matrix.M13,
			matrix.M21, matrix.M22, matrix.M23,
			matrix.M31, matrix.M32, matrix.M33);
		rotationMatrix.Orthonormalize();
		
		Quaternion.RotationMatrix(ref rotationMatrix, out Quaternion rotation);
		
		return FromRotationTranslation(rotation, translation);
	}

	public static DualQuaternion FromRotation(Quaternion rotation) {
		return new DualQuaternion(rotation, Quaternion.Zero);
	}

	public static DualQuaternion FromRotation(Quaternion rotation, Vector3 center) {
		//TODO: optimize
		return FromTranslation(-center)
			.Chain(FromRotation(rotation))
			.Chain(FromTranslation(+center));
	}

	public static DualQuaternion FromRotationTranslation(Quaternion rotation, Vector3 translation) {
		Quaternion real = rotation;
		Quaternion dual = 0.5f * (new Quaternion(translation.X, translation.Y, translation.Z, 0) * rotation);
		return new DualQuaternion(real, dual);
	}

	public static DualQuaternion FromTranslation(Vector3 translation) {
		Quaternion dual = 0.5f * new Quaternion(translation.X, translation.Y, translation.Z, 0);
		return new DualQuaternion(Quaternion.Identity, dual);
	}

	public Quaternion Rotation {
		get {
			return this.Real;
		}
	}

	public Vector3 Translation {
		get {
			Quaternion t = 2 * Dual * Quaternion.Invert(Real);
			return new Vector3(t.X, t.Y, t.Z);
		}
	}

	public Vector3 Transform(Vector3 v) {
		return Vector3.Transform(v, Rotation) + Translation;
	}
	
	public Vector3 InverseTransform(Vector3 v) {
		return Vector3.Transform(v - Translation, Quaternion.Invert(Rotation));
	}

	private static DualQuaternion Multiply(DualQuaternion dq1, DualQuaternion dq2) {
		return new DualQuaternion(
			dq1.Real * dq2.Real,
			dq1.Real * dq2.Dual + dq1.Dual * dq2.Real);
	}

	public DualQuaternion Chain(DualQuaternion dq2) {
		return Multiply(dq2, this);
	}
	
	public Matrix ToMatrix() {
		return Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Translation);
	}

	public DualQuaternion Invert() {
		var inverseRotation = Quaternion.Invert(Rotation);
		var inverseTranslation = Vector3.Transform(-Translation, inverseRotation);
		return DualQuaternion.FromRotationTranslation(inverseRotation, inverseTranslation);
	}

	public override string ToString() {
		return $"DualQuaternion[Rotation={Rotation.FormatForMathematica()}, Translation={Translation.FormatForMathematica()}]";
	}
}
