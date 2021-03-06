using System;
using SharpDX.Direct3D11;

public class Menu : IDisposable {
	private readonly MenuModel model;
	private readonly MenuController controller;
	private readonly MenuView visualRenderer;
	private readonly MenuRenderer renderer;
	
	public Menu(Device device, ShaderCache shaderCache, TrackedDeviceBufferManager trackedDeviceBufferManager, ControllerManager controllerManager, IMenuLevel rootLevel) {
		model = new MenuModel(rootLevel);
		controller = new MenuController(model, controllerManager);
		visualRenderer = new MenuView(device, model);
		renderer = new MenuRenderer(device, shaderCache, trackedDeviceBufferManager, controllerManager, visualRenderer.TextureView);
	}

	public void Dispose() {
		renderer.Dispose();
		visualRenderer.Dispose();
	}
	
	public void Update(DeviceContext context) {
		controller.Update();
		visualRenderer.Update(context);
		renderer.Update();
	}
	
	public void RenderPass(DeviceContext context, RenderingPass pass) {
		renderer.RenderPass(context, pass);
	}

	public void DoDrawCompanionWindowUi(DeviceContext context) {
		renderer.DoDrawCompanionWindowUi(context);
	}
	
	public void DoPrework(DeviceContext context) {
		visualRenderer.DoPrework(context);
	}

	public void DoPostwork(DeviceContext context) {
		visualRenderer.DoPostwork(context);
	}
}
