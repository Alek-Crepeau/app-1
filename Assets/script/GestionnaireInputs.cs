using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script qui r�cup�re la valeur des inputs (clavier et souris) � chaque update()
 * D�rive de MonoBehavior
 * Ex�cution locale. Les donn�es seront transmises au r�seau sur demande
 * Variables
 * - mouvementInputVecteur :Vector2 pour m�moriser axes vertical et horizontal
 * - vueInputVecteur : Vector2 pour m�moriser les d�placements de la souris, horizontal et vertical.
 * - ilSaute : bool qui sera activ�e lorsque le joueur saute
 * - GestionnaireCameraLocale : pour m�moriser le component GestionnaireCameraLocale de la cam�ra du joueur
 */

public class GestionnaireInputs : MonoBehaviour
{
    Vector2 mouvementInputVecteur = Vector2.zero;
    Vector2 vueInputVecteur = Vector2.zero;
    bool ilSaute;
    GestionnaireCameraLocale gestionnaireCameraLocale;
    public bool pretARecommencer;
    bool ilTir = false; // D�tection locale du clic de souris.
    GestionnaireMouvementPersonnage gestionnaireMouvementPersonnage; // r�f�rence au script de mouvement

    /*
     * Avant le Start(), on m�morise la r�f�rence au component GestionnaireCameraLocale de la cam�ra du joueur
     */
    void Awake()
    {
        gestionnaireCameraLocale = GetComponentInChildren<GestionnaireCameraLocale>();
        gestionnaireMouvementPersonnage = GetComponent<GestionnaireMouvementPersonnage>();
    }

    /*
     * On s'assure que le curseur est invisible et verrouill� au centre
     */
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /*
     * On m�morise � chaque frame la valeurs des inputs 
     *  - clavier : Axis Horizontal et vertical 
     *  - souris : Axis Mouse X et Mouse Y
     * Appel de la fonction SetInputVue dans le script GestionnaireCameraLocale en lui passant l'information
     * n�cessaire pour la rotation de la cam�ra (vueInputVecteur)
     * 2.Si la touche de saut est appuy�e, on met la variable ilSaute � true.
     */
    void Update()
    {
        // Optimisation : on ex�cute seulement le Update si le client contr�le ce joueur
        if (!gestionnaireMouvementPersonnage.Object.HasInputAuthority)
            return;

        // D�placement
        mouvementInputVecteur.x = Input.GetAxis("Horizontal");
        mouvementInputVecteur.y = Input.GetAxis("Vertical");

        // Vue
        vueInputVecteur.x = Input.GetAxis("Mouse X");
        vueInputVecteur.y = Input.GetAxis("Mouse Y");
        gestionnaireCameraLocale.SetInputVue(vueInputVecteur); // ligne a d�commenter plus tard

        //Saut
        if (Input.GetButtonDown("Jump"))
            ilSaute = true;

        //Tir
        if (Input.GetButtonDown("Fire1"))
            ilTir = true;

        // Si la partie n'est pas en cours (donc termin�e!) on �coute la toucher R. Quand la touche
        // est enfonc�e, on met la variable pretARecommencer � true.
        if (!GameManager.partieEnCours)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                pretARecommencer = true;
            }
        }

    }

    /*
     * Fonction qui sera appel�e par le Runner qui g�re la simulation (GestionnaireReseau). 
     * Lorsqu'elle est appel�e, son r�le est de :
     * 1. cr�er une structure de donn�es (struc) � partir du mod�le DonneesInputReseau;
     * 2. d�finir les trois variables de la structure (mouvement, vecteurDevant et saute);
     * Le vecteur de direction "vecteurDevant" est d�termin� par le forward de la cameraFPS
     * Une fois la donn�e de saut enregistr�e pour le input r�seau, on remet la variable ilSaute � false
     * 3. retourne au Runner la structure de donn�es
     */
    public DonneesInputReseau GetInputReseau()
    {
        //1.
        DonneesInputReseau donneesInputReseau = new DonneesInputReseau();

        //2.
        donneesInputReseau.mouvementInput = mouvementInputVecteur;
        donneesInputReseau.vecteurDevant = gestionnaireCameraLocale.gameObject.transform.forward;
        donneesInputReseau.saute = ilSaute;
        donneesInputReseau.appuieBoutonTir = ilTir;

        ilSaute = false;
        ilTir = false;

        donneesInputReseau.pretARejouer = pretARecommencer;

        //3.
        return donneesInputReseau;
    }
}

