public class PackArchiveDemo : IDemoApp {
	public void Run() {
		var packer = new ArchivePacker();
		packer.Pack(CommonPaths.WorkDir.File("packed.archive"), CommonPaths.WorkDir.Subdirectory("to-pack"));
	}
}
