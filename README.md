# 🧽 SharpWipe

**L'utilitaire C# natif et ultra-léger pour purger les métadonnées de vos vidéos en un éclair.**

SharpWipe est un outil Windows minimaliste conçu pour supprimer instantanément les métadonnées indésirables de vos fichiers vidéo, sans aucune perte de qualité ni réencodage. En utilisant directement l'API native de Windows (`IPropertyStore` via COM), il ne nécessite aucune installation ni dépendance tierce (comme FFmpeg ou ExifTool).

## ✨ Fonctionnalités

* **100% Natif & Autonome :** Écrit en C# pur, interagit directement avec `shell32.dll`. Aucun logiciel tiers requis.
* **Ultra-rapide :** Traite des dossiers entiers instantanément.
* **Zéro Perte de Qualité :** Ne touche pas aux flux vidéo ou audio, modifie uniquement le conteneur de métadonnées.
* **Ciblage Précis :** Efface les 4 propriétés les plus courantes qui polluent l'Explorateur Windows :
  * Titre (`System.Title`)
  * Description / Résumé (`System.Comment`)
  * Tags / Mots-clés (`System.Keywords`)
  * Catégorie (`System.Category`)

## 🎬 Formats Supportés

SharpWipe traite automatiquement les extensions suivantes :
`.mp4`, `.m4v`, `.mov`, `.avi`, `.wmv`, `.flv`, `.3gp`.

> **Note concernant les fichiers MKV (`.mkv`) :** > Le gestionnaire de propriétés (handler) natif de Windows pour le format MKV est souvent en lecture seule. SharpWipe tentera de les traiter, mais affichera un succès "Partiel" (comportement normal lié à l'OS).

## 🚀 Comment l'utiliser

SharpWipe est conçu pour être le plus simple possible :

### Méthode 1 : Glisser-Déposer (Drag & Drop)
Prenez simplement le dossier contenant vos vidéos et glissez-le sur l'icône de `SharpWipe.exe`. L'utilitaire s'ouvrira, nettoiera toutes les vidéos du dossier, et affichera le rapport.

### Méthode 2 : Ligne de commande (CLI)
Ouvrez votre terminal et lancez l'exécutable en lui passant le chemin du dossier cible en argument :
```cmd
SharpWipe.exe "C:\Chemin\Vers\MesVideos"
# SharpWipe
