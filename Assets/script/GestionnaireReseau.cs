using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System;


public class GestionnaireReseau : MonoBehaviour , INetworkRunnerCallbacks
{
    //Contient une r�f�rence au component NetworkRunner
    NetworkRunner _runner;
    //Index de la sc�ne du jeu
    public int IndexSceneJeu;
    // Contient la r�f�rence au script JoueurReseau du Prefab
    public JoueurReseau joueurPrefab;

    GestionnaireInputs gestionnaireInputs;

    // Tableau de couleurs � d�finir dans l'inspecteur
    public Color[] couleurJoueurs;
    // Pour compteur le nombre de joueurs connect�s
    public int nbJoueurs = 0;

    public SphereCollision sphereCollision; // r�f�rence au prefab de la boule rouge
    public bool spheresDejaSpawn; // Permet de savoir les boules ont d�j� �t� cr��es.


    void Start()
    {
        // Cr�ation d'une partie d�s le d�part
        // CreationPartie(GameMode.AutoHostOrClient);
    }

    // Fonction asynchrone pour d�marrer Fusion et cr�er une partie

    public async void CreationPartie(GameMode mode)
    {
        /*  1.M�morisation du component NetworkRunner . On garde en m�moire
            la r�f�rence � ce component dans la variable _runner.
            2.Indique au NetworkRunner qu'il doit fournir les entr�es (inputs) au 
            simulateur (Fusion)
        */
        _runner = gameObject.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        /*M�thode du NetworkRunner qui permet d'initialiser une partie
         * GameMode : re�u en argument. Valeur possible : Client, Host, Server,
           AutoHostOrClient, etc.)
         * SessionName : Nom de la chambre (room) pour cette partie
         * Scene : la sc�ne qui doit �tre utilis�e pour la simulation
         * SceneManager : r�f�rence au component script
          NetworkSceneManagerDefault qui est ajout� au m�me moment
         */
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = GameManager.instance.nomDeLapartie,
            Scene = SceneRef.FromIndex(IndexSceneJeu),
            PlayerCount = GameManager.instance.nombreDeJoueurMax,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //1.
        if (gestionnaireInputs == null && JoueurReseau.Local != null)
        {

            gestionnaireInputs = JoueurReseau.Local.GetComponent<GestionnaireInputs>();
        }

        //2.
        if (gestionnaireInputs != null)
        {
            input.Set(gestionnaireInputs.GetInputReseau());
        }

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    /* Lorsqu'un joueur se connecte au serveur
     * 1.On v�rifie si le code est ex�cut�e sur le serveur. Si c'est le cas, on spawn un prefab de joueur.
     * Bonne pratique : la commande Spawn() devrait �tre utilis�e seulement par le serveur
    */
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        print("OnPlayerjoin");
        if (_runner.IsServer)
        {
            Debug.Log("Un joueur s'est connect� comme serveur. Spawn d'un joueur");
            //_runner.Spawn(joueurPrefab, Utilitaires.GetPositionSpawnAleatoire(), Quaternion.identity, player);

            JoueurReseau leNouveuJoueur = _runner.Spawn(joueurPrefab, Utilitaires.GetPositionSpawnAleatoire(),
                                           Quaternion.identity, player);
            /*On change la variable maCouleur du nouveauJoueur et on augmente le nombre de joueurs connect�s
            Comme j'ai seulement 10 couleurs de d�finies, je m'assure de ne pas d�passer la longueur de mon
            tableau*/
            leNouveuJoueur.maCouleur = couleurJoueurs[nbJoueurs];
            nbJoueurs++;
            if (nbJoueurs >= 10) nbJoueurs = 0;

        }
        else
        {
            Debug.Log("Un joueur s'est connect� comme client. Spawn d'un joueur");
        }
    }



    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    /*
    * Fonction appel�e lorsqu'une connexion r�seau est refus�e ou lorsqu'un client perd
    * la connexion suite � une erreur r�seau. Le param�tre ShutdownReason est une �num�ration (enum)
    * contenant diff�rentes causes possibles.
    * Ici, lorsque la connexion est refus�e car le nombre maximal de joueurs est atteint, on appelle la
    * fonction NavigationPanel du GameManager en passant la valeur true en parm�tre.
    */
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (shutdownReason == ShutdownReason.GameIsFull)
        {
            GameManager.instance.NavigationPanel(true);
        }
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    /* Fonction ex�cut� sur le serveur seulement qui spawn le nombre de boules rouges d�termin�s au lancement
   d'une nouvelle partie.
   */
    public void CreationBoulleRouge()
    {
        if (_runner.IsServer && !spheresDejaSpawn)
        {
            GameManager.partieEnCours = true;
            for (int i = 0; i < GameManager.instance.nbBoulesRougesDepart; i++)
            {
                _runner.Spawn(sphereCollision, Utilitaires.GetPositionSpawnAleatoire(), Quaternion.identity);
            }
            spheresDejaSpawn = true;
        }
    }

    /*Fonction appel� pendant le jeu, lorsqu'il est n�cessaire de cr�er de nouvelles
  boule rouges. R�ception en param�tre du nombre de boules � cr�er.
  */
    public void AjoutBoulesRouges(int combien)
    {
        if (_runner.IsServer)
        {
            for (int i = 0; i < combien; i++)
            {
                _runner.Spawn(sphereCollision, Utilitaires.GetPositionSpawnAleatoire(), Quaternion.identity);
            }
        }
    }
}
