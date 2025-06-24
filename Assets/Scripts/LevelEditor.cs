using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Carrot;
using System.Linq;

sealed public class LevelEditor : MonoBehaviour {

    [Header("Obj Main")]
    public GameHandle game;
    public GameObject PlayerObj;

    [Header("Obj Level")]
    public static bool loadingLevel;
    public static bool loadingWonLevel;
    public static Texture2D testedLevelImage;

    [Header("Setting Form Level")]
    public Dropdown dropdownLevelType;
    public Slider RedInput;
    public Slider GreenInput;
    public Slider BlueInput;
    public Sprite Panel;
    public Sprite SelectedPanel;
    public Sprite questionMark;
    public Image imgColorShow;
    
    [Header("Obj Other")]
    public GameObject levelObjects;
	public GameObject widthArrows;
	public GameObject lengthArrows;
    public GameObject selectionPanel;
    public GameObject settingsPanel;
    public GameObject tinkerPanel;
    public GameObject exitPanel;
    public GameObject tooltipPanel;
    public GameObject screenshotPanel;
    public GameObject messagePanel;
    public GameObject messageText;
    public GameObject notificationText;
    public GameObject errorText;
    public GameObject tinkerStartDropdown;
    public GameObject playButton;

    //Level Settings Objects
    public string levelName;
    public string levelDifficulty;

    private int playerX;
    private int playerY;
    private Color32 levelColor;
	private RaycastHit hit;	
	private GameObject cursor;
	private GameObject currentObject;
	private GameObject currentArrow;
    private GameObject activePanel;
    private Transform currentButton;
	private Plane ground = new Plane(Vector3.up, Vector3.zero);
	private Vector3 mouseVector;
	private float hitDist;
	private bool validChoice;
	private bool cursorOOB;
    private bool levelComplete;
    private bool levelChanged;
    private bool minorChange;
    private bool helpEnabled;

	private int currentWidth;
	private int currentLength;

	private char selectedLayer;
	private char selectedBlock;
    private Mechanism selectedMechanism;
    private GameObject selectedMechObject;

	private char?[,] groundLayout = new char?[30,30];
	private char?[,] entityLayout = new char?[30,30];
	private char?[,] mechanismLayout = new char?[30,30];
	private Mechanism[,] mechanisms = new Mechanism[30,30];
    private Carrot_Box box;
    private GameObject ObjPlayerCur = null;

    private string[] LevelTypesSetting = new string[] {"Easy", "Medium", "Hard", "Expert"};

	void Start () {
        playerX = -1;

        if(!GameData.initialized) GameData.Initialize();

        SelectionSwitched("GF#Floor");
        activePanel = settingsPanel;
        levelColor = new Color32(162, 210, 190, 255);
        levelDifficulty = "Easy";

        if (loadingLevel)
        {
            LoadLevel(LevelLoader.JSONToLoad);
            levelComplete = loadingWonLevel;
            loadingWonLevel = false;
            loadingLevel = false;
        }
        else
        {
            currentWidth = 13;
            currentLength = 13;
            Camera.main.GetComponent<CameraControl>().SetPivotPoint(new Vector3(currentWidth - 1, 0, currentLength - 1));
        }
	}

	void Update () {
        if (!selectionPanel.activeSelf)
            return;
         
        if(tooltipPanel.activeSelf)
            tooltipPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x + (GameObject.Find("Tooltip Panel").GetComponent<RectTransform>().rect.width / 2), Input.mousePosition.y - Screen.height - (GameObject.Find("Tooltip Panel").GetComponent<RectTransform>().rect.height / 2));

        if (levelComplete && levelChanged)
            levelComplete = false;
        
		if(currentArrow) currentArrow.GetComponent<Renderer>().material.color = Color.white;

		if(Input.GetKeyDown(KeyCode.Backspace)) ClearLevel();

		Ray mouseRay = Camera.main.ScreenPointToRay (Input.mousePosition);
        if (cursor) Destroy(cursor);
        if (ground.Raycast(mouseRay, out hitDist) && ValidMousePos() && !helpEnabled)
		{
			#region Create Appropriate Cursor If Mouse Is Within Bounds Of Level
			mouseVector = mouseRay.GetPoint(hitDist);
			mouseVector = new Vector3(Mathf.Round(mouseRay.GetPoint(hitDist).x / 2) * 2,0,Mathf.Round(mouseRay.GetPoint(hitDist).z / 2) * 2);

			if((int)(mouseVector.x / 2) >= 0 && (int)(mouseVector.z / 2) >= 0 && (int)(mouseVector.x / 2) < currentWidth && (int)(mouseVector.z / 2) < currentLength)
			{
				cursorOOB = false;
				if(selectedLayer == 'G' && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null ||
                   selectedLayer == 'E' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'P' && mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'X' ||
                   selectedLayer == 'M' && selectedBlock != 'T' && mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'P' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'X' ||
                   selectedLayer == 'M' && selectedBlock == 'T' && mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null)
				{
					validChoice = true;
				}
				else
				{
					validChoice = false;
				}
			}
			else
			{
				cursorOOB = true;
			}

			if(!cursorOOB)
			{
				if(validChoice)
				{
					cursor = Instantiate(Resources.Load("ValidChoice"), (selectedLayer == 'G') ? mouseVector - new Vector3(0,1,0) : mouseVector + new Vector3(0,1,0), Quaternion.identity) as GameObject;
				}
				else
				{
					cursor = Instantiate(Resources.Load("InvalidChoice"), (selectedLayer == 'G') ? mouseVector - new Vector3(0,1,0) : mouseVector + new Vector3(0,1,0), Quaternion.identity) as GameObject;
				}
			}
			#endregion

			#region Instantiate Or Destroy A Level Object Based On Users Input
			if(Input.GetMouseButton(0) && validChoice && !cursorOOB)
			{
				if(selectedLayer == 'G')
				{
					currentObject = Instantiate(GameData.GroundTypes[selectedBlock], mouseVector - new Vector3(0, 1, 0), Quaternion.identity) as GameObject;
					groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = selectedBlock;
                    levelChanged = true;
                }
				else if(selectedLayer == 'E')
				{
					currentObject = Instantiate(GameData.EntityTypes[selectedBlock], mouseVector + new Vector3(0, 1, 0), Quaternion.identity) as GameObject;
					entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = selectedBlock;
                    levelChanged = true;

                    if (selectedBlock == 'P')
                    {
                        if (playerX != -1) StartCoroutine(DeleteBlock('E', playerX, playerY));
                        playerX = (int)(mouseVector.x / 2);
                        playerY = (int)(mouseVector.z / 2);
                        GameObject c = currentObject.transform.Find("Main Camera").gameObject;
                        Destroy(c);
                    }
				}
				else if(selectedLayer == 'M')
				{
                    if (selectedBlock != 'T')
                    {
                        currentObject = Instantiate(GameData.MechanismTypes[selectedBlock], mouseVector + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(-90, 0, 0))) as GameObject;
                        mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = selectedBlock;
                        mechanisms[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = currentObject.GetComponent<Mechanism>();
                        levelChanged = true;
                    }
                    else
                    {
                        Destroy(selectedMechObject);
                        selectedMechObject = Instantiate(Resources.Load("SelectedMechanism"), mouseVector + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(-90, 0, 0))) as GameObject;
                        selectedMechanism = mechanisms[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)];
                        OpenTinker();
                    }
                }

                if (!(selectedLayer == 'M' && selectedBlock == 'T'))
                {
                    currentObject.transform.parent = levelObjects.transform;
                    currentObject.name = selectedLayer + ((int)(mouseVector.x / 2)).ToString("00") + ((int)(mouseVector.z / 2)).ToString("00");
                    currentObject.GetComponent<Block>().Spawn(0, selectedBlock);
                }
			}
			else if(Input.GetMouseButton(1) && !validChoice && !cursorOOB)
			{
				if(selectedLayer == 'G' && (entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null || mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null))return;
                if(selectedLayer == 'E' && mechanismLayout[(int)(mouseVector.x / 2),(int)(mouseVector.z / 2)] != null)return;
                if (selectedLayer == 'M' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null)return;
                if (selectedLayer == 'E' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == 'P') playerX = -1;
                StartCoroutine(DeleteBlock(selectedLayer, (int)(mouseVector.x / 2), (int)(mouseVector.z / 2)));
			}
			#endregion
		}

		#region Level Size Control Via Arrows
		if(Physics.Raycast(mouseRay, out hit) && hit.collider.gameObject.tag == "EditorArrow" && ValidMousePos())
		{
			currentArrow = hit.collider.gameObject;
			currentArrow.GetComponent<Renderer>().material.color = Color.yellow;

			if(Input.GetMouseButtonDown(0))
			{
				int sizeModifier = (currentArrow.name.Contains("Increase")) ? 1 : -1;
				if(currentArrow.transform.parent.gameObject == lengthArrows && (sizeModifier == 1 && currentLength < 29 || sizeModifier == -1 && currentLength > 4))
				{
					currentLength += sizeModifier;
					iTween.MoveTo(lengthArrows, iTween.Hash("position", new Vector3(currentWidth - 3,0,(currentLength * 2) + 1), "time", .5f));
					iTween.MoveTo(widthArrows, iTween.Hash("position", new Vector3((currentWidth * 2) + 1,0,currentLength + 1), "time", .5f));
					Camera.main.GetComponent<Grid>().ChangeGridSize(false, sizeModifier * 2);

					if(sizeModifier == -1)
						ClearLine(true, currentLength, currentWidth);

					Camera.main.GetComponent<CameraControl>().ChangePivotPoint(new Vector3(0,0, sizeModifier));
					CheckArrows();
				}
				else if(currentArrow.transform.parent.gameObject == widthArrows && (sizeModifier == 1 && currentWidth < 29 || sizeModifier == -1 && currentWidth > 4))
				{
					currentWidth += sizeModifier;
					iTween.MoveTo(lengthArrows, iTween.Hash("position", new Vector3(currentWidth - 3,0,(currentLength * 2) + 1), "time", .5f));
					iTween.MoveTo(widthArrows, iTween.Hash("position", new Vector3((currentWidth * 2) + 1,0,currentLength + 1), "time", .5f));
					Camera.main.GetComponent<Grid>().ChangeGridSize(true, sizeModifier * 2);

					if(sizeModifier == -1)
						ClearLine(false, currentWidth, currentLength);

					Camera.main.GetComponent<CameraControl>().ChangePivotPoint(new Vector3(sizeModifier,0, 0));
					CheckArrows();
				}
                levelChanged = true;
            }
        }
		#endregion
	}

	void ClearLine(bool row, int number, int amount)
	{
		#region Clear An Entire Row Or Column If Reducing Level Size
		for(int i = 0; i <= amount ; i++)
		{
			if(groundLayout[row? i : number, row? number : i] != null)
				StartCoroutine(DeleteBlock('G', row? i : number, row? number : i));
			if(entityLayout[row? i : number, row? number : i] != null)
				StartCoroutine(DeleteBlock('E', row? i : number, row? number : i));
			if(mechanismLayout[row? i : number, row? number : i] != null)
				StartCoroutine(DeleteBlock('M', row? i : number, row? number : i));
		}
		#endregion
	}

	IEnumerator DeleteBlock(char layer, int x, int y)
	{
        #region Delete Specific Block Found Using Coordinates

		if(levelObjects.transform.Find(layer + x.ToString("00") + y.ToString("00")))
		{
            if (levelObjects.transform.Find(layer + x.ToString("00") + y.ToString("00")).GetComponent<Block>().Despawning)
                yield break;

            levelChanged = true;
            levelObjects.transform.Find(layer + x.ToString("00") + y.ToString("00")).GetComponent<Block>().Despawn();
			yield return new WaitForSeconds(0.5f);
			if(layer == 'G')
				groundLayout[x,y] = null;
			else if(layer == 'E')
				entityLayout[x,y] = null;
			else if(layer == 'M')
				mechanismLayout[x,y] = null;
		}
		else
		{
			Debug.Log("No block found at: " + layer + x.ToString("00") + y.ToString("00"));
		}
		#endregion
	}

    private void ClearLevel()
    {
        ClearTinker();
        playerX = -1;
        StartCoroutine(DisablePlayUpload());

        for (int x = 0; x < currentWidth; x++)
        {
            for (int y = 0; y < currentLength; y++)
            {
                if (groundLayout[x, y] != null)
                    StartCoroutine(DeleteBlock('G', x, y));
                if (entityLayout[x, y] != null)
                    StartCoroutine(DeleteBlock('E', x, y));
                if (mechanismLayout[x, y] != null)
                    StartCoroutine(DeleteBlock('M', x, y));
            }
        }
    }

    private IEnumerator DisablePlayUpload()
    {
        playButton.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(2.5f);
        playButton.GetComponent<Button>().interactable = true;
    }

    private void ClearTinker()
    {
        if (selectedMechObject)
            Destroy(selectedMechObject);

        if (tinkerPanel.activeSelf)
            tinkerPanel.SetActive(false);
    }

	void CheckArrows()
	{
		#region Arrow Enabling/Disabling based on level size
		//Width too low
		if(currentWidth <= 4)
			widthArrows.transform.Find("Width Decrease").gameObject.SetActive(false);
		else if(!widthArrows.transform.Find("Width Decrease").gameObject.activeSelf)
			widthArrows.transform.Find("Width Decrease").gameObject.SetActive(true);
		
		//Width too high
		if(currentWidth >= 29)
			widthArrows.transform.Find("Width Increase").gameObject.SetActive(false);
		else if(!widthArrows.transform.Find("Width Increase").gameObject.activeSelf)
			widthArrows.transform.Find("Width Increase").gameObject.SetActive(true);
		
		//Length too low
		if(currentLength <= 4)
			lengthArrows.transform.Find("Length Decrease").gameObject.SetActive(false);
		else if(!lengthArrows.transform.Find("Length Decrease").gameObject.activeSelf)
			lengthArrows.transform.Find("Length Decrease").gameObject.SetActive(true);
		
		//Length too high
		if(currentLength >= 29)
			lengthArrows.transform.Find("Length Increase").gameObject.SetActive(false);
		else if(!lengthArrows.transform.Find("Length Increase").gameObject.activeSelf)
			lengthArrows.transform.Find("Length Increase").gameObject.SetActive(true);
		#endregion
	}

	public void SelectionSwitched(string s)
	{
        //Split s into array, one containing selected layer and block characters, and one containing the button name
        string[] contents = s.Split('#');

        if (contents.Length != 2 || contents[0].Length != 2)
			return;

        //Clean up gameobjects left over from previous mechanism tinkering
        if (selectedMechObject != null)
            Destroy(selectedMechObject);
        if (tinkerPanel.activeSelf)
            tinkerPanel.SetActive(false);

        if (contents[1] == "Help")
        {
            helpEnabled = true;
            ShowTooltip("Help");
        }
        else
        {
            helpEnabled = false;
            HideTooltip();
            selectedLayer = contents[0][0];
            selectedBlock = contents[0][1];
        }

        if (currentButton) currentButton.GetComponent<Image>().sprite = Panel;
        currentButton = GameObject.Find(contents[1] + " Button").transform;
        currentButton.GetComponent<Image>().sprite = SelectedPanel;
    }

    Dictionary<string, object> levelAsJSON()
	{
		Dictionary<string, object> levelData = new Dictionary<string, object>();

		string groundLayer = "";
		string entityLayer = "";
		string mechanismLayer = "";
        string mechanismsText = "";

		int minX = -1;
		int maxX = currentWidth;
		int minY = -1;
		int maxY = currentLength;

		#region Determine Actual Level Dimensions By Removing Rows/Columns Of Free Space
		for(int x = 0; x < currentWidth; x++)
		{
			for(int y = 0; y < currentLength; y++)
			{
				if(groundLayout[x,y] != null && entityLayout != null && mechanismLayout != null)
				{
					if(minX == -1)
						minX = x;
					maxX = x;
					break;
				}
			}
		}

        if (minX == -1) return levelData;

        for (int y = 0; y < currentLength; y++)
		{
			for(int x = 0; x < currentWidth; x++)
			{
				if(groundLayout[x,y] != null && entityLayout != null && mechanismLayout != null)
				{
					if(minY == -1)
						minY = y;
					maxY = y;
					break;
				}
			}
		}
		#endregion

		Debug.Log(maxY.ToString("00") + minY.ToString("00") + "  " + maxX.ToString("00") + minX.ToString("00"));
		Debug.Log((maxY - minY).ToString ("00") + (maxX - minX).ToString ("00"));

		for(int y = minY; y <= maxY; y++)
		{
			for(int x = minX; x <= maxX; x++)
			{
				if(groundLayout[x,y] == null)
					groundLayer += 'Z';
				else
					groundLayer += groundLayout[x,y];

				if(entityLayout[x,y] == null)
					entityLayer += 'Z';
				else
					entityLayer += entityLayout[x,y];

				if(mechanismLayout[x,y] == null)
					mechanismLayer += 'Z';
				else
					mechanismLayer += mechanismLayout[x,y];

                if(mechanisms[x,y] != null)
                {
                    mechanismsText += mechanisms[x, y].group.ToString();
                    if (mechanisms[x, y].receivesInput)
                        mechanismsText += mechanisms[x, y].startOpen ? "1" : "0";
                    else
                        mechanismsText += "Z";
                }
			}
		}

		levelData.Add("id", "L0003");
		levelData.Add("name", levelName);
        levelData.Add("difficulty", levelDifficulty);
		levelData.Add("creator", "Michael");
		levelData.Add("colour", levelColor.r.ToString("000") + levelColor.g.ToString("000") + levelColor.b.ToString("000"));
		levelData.Add("dimensions", (maxX - minX + 1).ToString("00") + (maxY - minY + 1).ToString("00"));
        levelData.Add("groundlayer", Crypto.Compress(groundLayer));
        levelData.Add("entitylayer", Crypto.Compress(entityLayer));
        levelData.Add("mechanismlayer", Crypto.Compress(mechanismLayer));
        levelData.Add("mechanisms", mechanismsText);

        return levelData;
	}

    private IEnumerator SaveLevelWait()
    {
        Message("Saving...", false);
        yield return new WaitForSeconds(2);
        Debug.Log("Save data:" + Json.Serialize(levelAsJSON()));
        this.game.mLevel.Add(Json.Serialize(levelAsJSON()));
        LevelActionSelect("Back");
    }

    public void TestLevel()
    {
        if (tooltipPanel.activeSelf)
        {
            if (helpEnabled)
            {
                helpEnabled = false;
                ShowRequirements(false);
                SelectionSwitched("GF#Floor");
                return;
            }
            else
                return;
        }

        bool finishExists = false;
        for (int y = 0; y < currentLength; y++)
        {
            for (int x = 0; x < currentWidth; x++)
            {
                if (groundLayout[x, y] == 'X')
                    finishExists = true;
            }
        }
        if (!finishExists)return;

        LevelLoader.levelToLoad = -1;
        LevelLoader.JSONToLoad = levelAsJSON();
        this.game.OnShowPlayTestLevelEditor(levelAsJSON());
    }

    private bool ValidMousePos()
    {
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < 250 && (Screen.height - mousePos.y) < (125 + (DropDownList.droppedCount * 75)))
            return false;
        else 
            return true;
    }

    public void SaveLevelSettings()
    {
        if (levelColor != GameObject.Find("Level Color Select").GetComponent<Image>().color || levelDifficulty!=LevelTypesSetting[dropdownLevelType.value]) minorChange = true;
        levelName = GameObject.Find("Level Name Input").GetComponent<InputField>().text;
        Camera.main.backgroundColor = GameObject.Find("Level Color Select").GetComponent<Image>().color;
        levelColor = GameObject.Find("Level Color Select").GetComponent<Image>().color;
        levelDifficulty = LevelTypesSetting[dropdownLevelType.value];
        LevelActionSelect("Back");
    }

    public void CancelLevelSettings()
    {
        GameObject.Find("Level Name Input").GetComponent<InputField>().text = levelName;
        GameObject.Find("Level Color Select").GetComponent<Image>().color = (Color)levelColor;
        RedInput.value = levelColor.r;
        GreenInput.value = levelColor.g;
        BlueInput.value = levelColor.b;
        LevelActionSelect("Back");
    }

    public void LevelActionSelect(string action)
    {
        if(action == "Settings")
        {
            settingsPanel.SetActive(true);
            ClearTinker();
            activePanel = settingsPanel;
            this.dropdownLevelType.options.Clear();
            for (int i = 0; i < this.LevelTypesSetting.Length; i++)
            {
                this.dropdownLevelType.options.Add(new Dropdown.OptionData(this.LevelTypesSetting[i]));
                if(levelDifficulty== this.LevelTypesSetting[i])this.dropdownLevelType.value = i;
            }
            this.dropdownLevelType.RefreshShownValue();
            GameObject.Find("Level Name Input").GetComponent<InputField>().text = levelName;
            GameObject.Find("Level Color Select").GetComponent<Image>().color = (Color)levelColor;
            RedInput.value = levelColor.r;
            GreenInput.value = levelColor.g;
            BlueInput.value = levelColor.b;
            Camera.main.GetComponent<CameraControl>().disableRotation = true;
            selectionPanel.SetActive(false);
        }
        else if(action == "Exit")
        {
            if (levelChanged || minorChange)
            {
                exitPanel.SetActive(true);
                activePanel = exitPanel;
                Camera.main.GetComponent<CameraControl>().disableRotation = true;
                selectionPanel.SetActive(false);
            }
            else
                ExitEditor();

        }
        else if(action == "Save")
        {
            if(levelName == "")
                SetNotification("You need to set a level name in order to save the level");
            else
                StartCoroutine(SaveLevelWait());
        }
        else if(action == "Load")
        {
            ClearTinker();
            selectionPanel.SetActive(false);
            Camera.main.GetComponent<CameraControl>().disableRotation = true;
            PopulateLevelList();
        }
        else if(action == "Back")
        {
            tooltipPanel.SetActive(false);
            activePanel.SetActive(false);
            selectionPanel.SetActive(true);
            activePanel = selectionPanel;
            Camera.main.GetComponent<CameraControl>().disableRotation = false;
        }
        else if(action == "Upload")
        {
            if (tooltipPanel.activeSelf)
            {
                if (helpEnabled)
                {
                    helpEnabled = false;
                    ShowRequirements(false);
                    SelectionSwitched("GF#Floor");
                    return;
                }
                else
                    return;
            }

            //TEMP ALPHA V0.1.0 PLEASE DELETE
            bool finishExists = false;
            for (int y = 0; y < currentLength; y++)
            {
                for (int x = 0; x < currentWidth; x++)
                {
                    if (groundLayout[x, y] == 'X')
                        finishExists = true;
                }
            }
            if (!finishExists)
                return;
            //----

            selectionPanel.SetActive(false);
            Camera.main.GetComponent<CameraControl>().disableRotation = true;
        }
        else if(action == "Clear")
        {
            ClearLevel();
        }
    }

    private void PopulateLevelList()
    {
        if (box != null) box.close();
        this.box=this.game.carrot.Create_Box("Load Level");
        this.box.set_icon(this.game.carrot.icon_carrot_all_category);
        List<Dictionary<string, object>> listLevel = this.game.mLevel.GetListLevel();
        for (int i = 0; i < listLevel.Count; i++)
        {
            var index_item = i;
            Dictionary<string, object> dataL = listLevel[i];
            var d = dataL;
            var d_name= dataL["name"].ToString();
            Carrot_Box_Item item_lv = box.create_item();
            item_lv.set_icon(this.game.carrot.icon_carrot_database);
            item_lv.set_title(dataL["name"].ToString());
            item_lv.set_tip(dataL["difficulty"].ToString());
            item_lv.set_act(() =>
            {
                if (this.box != null) this.box.close();
                this.game.carrot.play_sound_click();
                StartCoroutine(LoadLevelWait(d));
            });

            Carrot_Box_Btn_Item btn_del = item_lv.create_item();
            btn_del.set_icon(game.carrot.sp_icon_del_data);
            btn_del.set_icon_color(Color.white);
            btn_del.set_color(game.carrot.color_highlight);
            btn_del.set_act(()=>
            {
                game.carrot.Show_msg("Delete Level","Are you sure you want to delete this level '"+d_name+"'?", () =>
                {
                    game.carrot.play_sound_click();
                    game.mLevel.DeleteItem(index_item);
                    PopulateLevelList();
                });
            });
        }
        box.set_act_before_closing(() =>
        {
            this.LevelActionSelect("Back");
        });
    }

    IEnumerator LoadLevelWait(Dictionary<string, object> dataLevel)
    {
        ClearLevel();
        yield return new WaitForSeconds(2);
        if(ObjPlayerCur != null) Destroy(ObjPlayerCur);
        this.ObjPlayerCur=Instantiate(this.PlayerObj, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        this.ObjPlayerCur.name = "Player";
        GameObject c = ObjPlayerCur.transform.Find("Main Camera").gameObject;
        Destroy(c);
        LoadLevel(dataLevel);
        LevelActionSelect("Back");
        SetNotification("Loaded Level \"" + levelName + "\"");
    }

    public void ColourTextChanged(string id)
    {
        int colourVal = 0;

        if (id == "R")
            colourVal = (int)RedInput.value;
        else if (id == "G")
            colourVal = (int)GreenInput.value;
        else if (id == "B")
            colourVal = (int)BlueInput.value;

        this.imgColorShow.color = new Color(id == "R" ? colourVal / 255f : this.imgColorShow.color.r, id == "G" ? colourVal / 255f : this.imgColorShow.color.g, id == "B" ? colourVal / 255f : this.imgColorShow.color.b);
    }

    public void ExitEditor()
    {
        if(ObjPlayerCur!=null) Destroy(ObjPlayerCur);
        game.OnBtn_Home();
    }

    public void SaveExit()
    {
        if (levelName == "")
        {
            LevelActionSelect("Back");
            SetNotification("You need to set a level name in order to save the level");
        }
        else
        {
            StartCoroutine(SaveLevelWait());
            ExitEditor();
        }
    }

    private void Message(string message, bool error)
    {
        activePanel.SetActive(false);
        messagePanel.SetActive(true);

        messageText.SetActive(error? false : true);
        errorText.SetActive(error? true : false);

        if (error)
            errorText.GetComponent<Text>().text = message;
        else
            messageText.GetComponent<Text>().text = message;

        activePanel = messagePanel;
        messagePanel.transform.Find("Okay Button").gameObject.SetActive(error ? true : false);
    }

    
    private void OpenTinker()
    {
        tinkerPanel.SetActive(true);
        tinkerPanel.transform.Find("Group Dropdown").GetComponent<Dropdown>().value = selectedMechanism.group;

        if(tinkerPanel.transform.Find("Chosen Mechanism").childCount > 0)
            Destroy(tinkerPanel.transform.Find("Chosen Mechanism").GetChild(0).gameObject);

        currentObject = Instantiate(GameData.MechanismTypes[selectedMechanism.ID], tinkerPanel.transform.Find("Chosen Mechanism").position, Quaternion.Euler(300,180,135)) as GameObject;
        currentObject.transform.parent = tinkerPanel.transform.Find("Chosen Mechanism");
        currentObject.transform.localScale = new Vector3(1,1,1);
        currentObject.GetComponent<Renderer>().material.mainTexture = Resources.Load("Textures\\" + GameData.MechanismTypes[selectedMechanism.ID].name + selectedMechanism.GetComponent<Mechanism>().group.ToString()) as Texture;

        tinkerPanel.transform.Find("Mechanism Name").GetComponent<Text>().text = GameData.MechanismTypes[selectedMechanism.ID].name;
    }

    public void TinkerGroupChanged(int group)
    {
        levelChanged = true;
        selectedMechanism.group = (byte)(tinkerPanel.transform.Find("Group Dropdown").GetComponent<Dropdown>().value);
        selectedMechanism.GetComponent<Renderer>().material.mainTexture = Resources.Load("Textures\\" + GameData.MechanismTypes[selectedMechanism.ID].name + selectedMechanism.GetComponent<Mechanism>().group.ToString()) as Texture;
        tinkerPanel.transform.Find("Chosen Mechanism").GetChild(0).GetComponent<Renderer>().material.mainTexture = Resources.Load("Textures\\" + GameData.MechanismTypes[selectedMechanism.ID].name + selectedMechanism.GetComponent<Mechanism>().group.ToString()) as Texture;
    }

    private void ShowRequirements(bool upload)
    {
        string requirements = "";
        int lineCount = 0;
        int playerCount = 0;
        bool finishExists = false;

        for(int y = 0; y < currentLength; y++)
        {
            for(int x = 0; x < currentWidth; x++)
            {
                if(groundLayout[x, y] == 'X')
                    finishExists = true;
                if(entityLayout[x, y] == 'P')
                    playerCount++;
            }
        }

        if (!finishExists)
        {
            requirements += "- There must be at least one finish block\n";
            lineCount += 2;
        }
        if (playerCount == 0)
        {
            requirements += "- There must be a player entity\n";
            lineCount += 2;
        }
        if(playerCount > 1)
        {
            requirements += "- There can only be one player entity\n";
            lineCount += 2;
        }
        if (upload)
        {
            if (levelName == "")
            {
                requirements += "- You must set a level name in settings\n";
                lineCount += 2;
            }
            if (!levelComplete)
            {
                requirements += "- You must complete the level yourself before uploading it\n";
                lineCount += 3;
            }     
        }

        if (requirements == "") return;

        tooltipPanel.SetActive(true);
        tooltipPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 8 + (16 * lineCount));
        tooltipPanel.transform.GetChild(0).GetComponent<Text>().text = requirements;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    public void ShowTooltip(string id)
    {
        if (!helpEnabled)
        {
            if (id == "Play") ShowRequirements(id == "Play" ? false : true);
            return;
        }

        string helpString = "";
        int lineCount = 0;

        switch(id)
        {
            case "Settings":
                helpString = "Allows you to change the name, intended difficulty and color of the level.";
                lineCount = 3;
                break;
            case "Save":
                helpString = "Allows you to save the level, providing they have set a level name.";
                lineCount = 3;
                break;
            case "Play":
                helpString = "Play through the level you have built to test its elements.";
                lineCount = 3;
                break;
            case "Load":
                helpString = "Open previous finished or part-built levels and edit them.";
                lineCount = 3;
                break;
            case "Tinker":
                helpString = "Select mechanisms and change their more intricate settings.";
                lineCount = 3;
                break;
            case "Exit":
                helpString = "Exit the level editor and return to the main menu.";
                lineCount = 2;
                break;
            case "Help":
                helpString = "Selecting this and highlighting other buttons will show you what they do.";
                lineCount = 4;
                break;
            case "Clear":
                helpString = "Clear the current level elements.";
                lineCount = 2;
                break;
            case "Floor":
                helpString = "A standard floor block, players can move on this without any side effects.";
                lineCount = 3;
                break;
            case "Ice":
                helpString = "An ice block, when players move onto ice, they cannot stop until they are not on an ice block.";
                lineCount = 5;
                break;
            case "Pit":
                helpString = "A pit, pushing crates into this will create a platform that acts as a floor block.";
                lineCount = 3;
                break;
            case "Finish":
                helpString = "A finish block, when a player moves onto this, the level is complete.";
                lineCount = 3;
                break;
            case "Wall":
                helpString = "A wall block, acts as an immovable obstruction to any entities.";
                lineCount = 3;
                break;
            case "Crate":
                helpString = "A standard crate, can be moved when pushed by players or when another crate is pushed into it on ice.";
                lineCount = 5;
                break;
            case "Heavy Crate":
                helpString = "A heavy crate, at the moment, it behaves exactly the same as a normal crate.";
                lineCount = 4;
                break;
            case "Player":
                helpString = "The player object, only one of these can exist at a time.";
                lineCount = 3;
                break;
            case "Button":
                helpString = "A button, can be pushed down by an entity above it. When all buttons of one color are pushed down, the corresponding door(s) will be activated.";
                lineCount = 6;
                break;
            case "Door":
                helpString = "A door, will be activated when all buttons of the same color are pushed down.";
                lineCount = 4;
                break;
        }

        if(helpString != "")
        {
            tooltipPanel.SetActive(true);
            tooltipPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 8 + (16 * lineCount));
            tooltipPanel.transform.GetChild(0).GetComponent<Text>().text = helpString;
        }
    }


    private void SetNotification(string message)
    {
        notificationText.SetActive(true);
        notificationText.GetComponent<NotificationMessage>().SetMessage(message);
    }

    private void LoadLevel(Dictionary<string, object> levelData)
    {
        Level levelToLoad = LevelLoader.LoadFromJSON(levelData, -1, true);
        levelColor = levelToLoad.BackgroundColor;

        levelName = levelToLoad.Name;
        levelDifficulty = levelToLoad.Difficulty;

        if (levelToLoad.Width < 4)
            currentWidth = 4;
        else
            currentWidth = levelToLoad.Width;

        if (levelToLoad.Length < 4)
            currentLength = 4;
        else
            currentLength = levelToLoad.Length;

        for (int y = 0; y < levelToLoad.Length; y ++)
        {
            for(int x = 0; x < levelToLoad.Width; x++)
            {
                if(levelToLoad.GroundLayout[x,y] != 'Z')
                    groundLayout[x, y] = levelToLoad.GroundLayout[x, y];
                if(levelToLoad.EntityLayout[x, y] != null)
                {
                    if (levelToLoad.EntityLayout[x, y].ID == 'P')
                    {
                        playerX = x;
                        playerY = y;
                    }
                    entityLayout[x, y] = levelToLoad.EntityLayout[x, y].ID;
                }
                if (levelToLoad.MechanismLayout[x, y] != null)
                {
                    mechanismLayout[x, y] = levelToLoad.MechanismLayout[x, y].ID;
                    mechanisms[x, y] = levelToLoad.MechanismLayout[x, y];
                }
                
				Camera.main.GetComponent<Grid>().SetGridSize((currentWidth * 2) - 1, (currentLength * 2) - 1);
				lengthArrows.transform.position = new Vector3(currentWidth - 3,0,(currentLength * 2) + 1);
				widthArrows.transform.position = new Vector3((currentWidth * 2) + 1,0,currentLength + 1);
				CheckArrows();
            }
        }
        Camera.main.GetComponent<CameraControl>().SetPivotPoint(new Vector3(currentWidth - 1, 0, currentLength - 1));
        levelChanged = false;
        levelComplete = false;
    }
}