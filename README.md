# A Nodal Dialogs Editor For Unity Engine!

[FR]

Bonjour à tous! 
J'ai créé un outil  d'édition de dialogues inspiré de cette conférence animée par Anna Kipnis de Double Fine Productions à la GDC 2015. Il est fortement recommandé de la regarder puisque j'utilise beaucoup de mécanismes présentés dans cette conférence (et en plus, elle est très intéressante!).

https://www.youtube.com/watch?v=0hMiPBe_VRc

Passons maintenant aux choses sérieuses! 

Avant tout pour commencer, comment ouvrir la fenêtre d'édition des dialogues ? Simplement, vous pouvez trouver dans le menu déroulant une section "Dialog Editor" et en cliquant sur Open Editor vous ouvrirez donc la fenêtre d'édition de dialogues.

![image-20200302102349351](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302102349351.png)

## I. Créer un Dialog Asset

Une fois sur la fenêtre d'édition de dialogues, vous pouvez faire un clic droit pour ouvrir un menu qui vous proposera deux options: créer ou ouvrir un dialogue. Nous allons donc en créer un.

![image-20200302103111533](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302103111533.png)

![image-20200302104114621](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302104114621.png)

Ici, vous avez donc deux champs à remplir:
**<u>Dialog Name</u>** : Ceci sera le nom de votre dialogue, vous pourrez le retrouver dans le dossier Assets/DialogsDatas/Dialogs.
**<u>Spreadsheet ID</u>**: Ici vous devrez renseigner l'ID de partage de votre Google Sheets. Vous pouvez le trouver en cliquant sur le bouton "Partager" en haut à droite de votre feuille de dialogues. Vous copierez donc un lien de cette forme https:/docs.google.com/spreadsheets/d/[spreadsheet_ID]/edit?usp=sharing et c'est cet ID qu'il faudra coller dans le champ Spreadsheet ID.
Puis en cliquant sur le bouton "Create Dialog And Load Spreadsheet" vous allez donc créer un Dialog Asset et télécharger le fichier TSV de votre spreadsheet sur votre ordinateur. Si vous avez apporté des modifications à votre spreadsheet, vous pouvez la mettre à jour en faisant un clic droit et en cliquant sur "Update Spreadsheet".

[Concernant la Spreadsheet]: La spreadsheet doit être sous cette forme afin de fonctionner avec cet outil.

![image-20200302114008035](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302114008035.png)

Vous pouvez ensuite ajouter autant de langues que vous le souhaitez, mais il faut que cela reste sous cette forme avec les IDs, suivi des lignes de dialogues rangées en fonctions de chaque clé de localisation.

## II. Ajouter des nœuds à votre dialogue.

Maintenant que votre dialogue est créé, vous pouvez désormais commencer à l'éditer. Pour cela vous pouvez créer des nœuds en faisant un clic droit, il en existe 4 sortes:

1. <u>Starting Node</u>
   ![image-20200302111839732](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302111839732.png)

   Le Start Node est utilisé pour savoir où le dialogue va commencer, il est présent par défaut et ne peut pas être supprimé. Vous pouvez ajouter des sorties à ce nœud en ajoutant des entrées à l'enum DialogStarterEnum dans le script EnumHolder.cs. 
   ![image-20200302112148871](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302112148871.png)

   Vous pourrez ensuite préciser vers quels nœuds poursuivre en fonction de l'enum choisi au moment de la lecture du dialogue (Cette partie sera expliquée plus en détail plus tard).
   

2. <u>Basic Node</u>
   ![image-20200302112524804](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302112524804.png)

   Le Basic Node est le nœud de base des dialogues, il peut prendre une ou plusieurs lignes de dialogues.
   Chaque ligne de dialogue possède plusieurs paramètres:

   - <u>Line ID</u>: La clé d'identification de la ligne dans la spreadsheet du dialogue. Elle sera utilisée pour retrouver le contenu de la ligne de dialogue, et ce en fonction du langage sélectionné dans les options.
   - <u>Inital Waiting Time</u>: Le temps d'affichage de la ligne de dialogue à l'écran. Si il y a un AudioClip lié à cette ligne de dialogue, ce temps d'affichage sera égal à la durée de l'audio clip, sinon il sera égal à la durée précisée dans ce champ.
   - <u>Extra Waiting Type</u>: Il s'agit de type d'attente qui sera exécuté après le temps d'affichage de la ligne. Il peut s'agir d'un temps d'attente, l'attente d'un input par le joueur ou alors ne rien attendre. 

   En plus de cela, le Basic Node a lui aussi des paramètres:

   - Un premier pour choisir s'il doit jouer tout le set de lignes de dialogues ou seulement une seule.
   - Un second pour choisir s'il doit jouer son set de lignes de dialogues aléatoirement ou séquentiellement.
     

3. <u>Answer Node</u>
   L'Answer Node est une variante du Basic Node, on peut l'ajouter en faisant un clic droit sur un Basic Node ou en cliquant sur l'icône en haut à droite du nœud.
   ![image-20200302114608984](C:\Users\Brassart_S03_Alien3\AppData\Roaming\Typora\typora-user-images\image-20200302114608984.png)

4. <u>Condition Node</u> 
