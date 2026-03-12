using CarPlay;
using Foundation;
using UIKit;

namespace TuneGames;

[Register("CarPlaySceneDelegate")]
public class CarPlaySceneDelegate : CPTemplateApplicationSceneDelegate
{
    CPInterfaceController? interfaceController;
    CarPlayMenuManager? menuManager;
    CarPlayGameManager? gameManager;

    public override void DidConnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController @interfaceController)
    {
        this.interfaceController = interfaceController;
        this.menuManager = new CarPlayMenuManager(interfaceController, this.StartGame);
        this.menuManager.Show();
    }

    public override void DidDisconnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController @interfaceController)
    {
        this.gameManager?.Cleanup();
        this.gameManager = null;
        this.menuManager = null;
        this.interfaceController = null;
    }

    void StartGame(string categoryName)
    {
        if (this.interfaceController == null)
            return;

        this.gameManager?.Cleanup();
        this.gameManager = new CarPlayGameManager(this.interfaceController, this.OnGameExit);
        this.gameManager.StartGame(categoryName);
    }

    void OnGameExit()
    {
        this.gameManager?.Cleanup();
        this.gameManager = null;
        this.menuManager?.Show();
    }
}
