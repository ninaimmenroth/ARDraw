using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class ModeManager : MonoBehaviour
{
    public bool drawMode, selectMode, dragMode;

    public GameObject modalWindow, secondPanel;

    private bool savedColorSelected = false;
    public FlexibleColorPicker fcp;
    private Color selectedColor, bufferColor, inactiveColor, activeColor;

    private float selectedWidth;
    
    public GameObject button1, button2, button3, button4, button5, colorButton, toolsButton, background1, background2;
    private int index = 1;
    private Vector3 scaleChange = new Vector3(0.05f, 0.05f, 0.0f);

    // Start is called before the first frame update
    void Start()
    {
        drawMode = true;
        selectMode = false;
        dragMode = false;
        selectedColor = fcp.color;
        bufferColor = selectedColor;
        inactiveColor = new Color(0.37f, 0.47f, 0.62f, 1.0f);
        activeColor = toolsButton.GetComponent<Image>().color;
        selectedWidth = 0.01f;
    }

    void Update(){
        if(savedColorSelected){
            if(fcp.color != bufferColor){
                savedColorSelected = false;
                selectedColor = fcp.color;
                bufferColor = selectedColor; 
                colorButton.GetComponent<Image>().color = selectedColor;
            }
        } else {
            selectedColor = fcp.color;
            bufferColor = selectedColor;
            colorButton.GetComponent<Image>().color = selectedColor;
        }
    }

    public void handleSaveColor(){
        switch(index){
            case 1:
                button1.GetComponent<Image>().color = fcp.color;
                break;
            case 2:
                button2.GetComponent<Image>().color = fcp.color;
                break;
            case 3:
                button3.GetComponent<Image>().color = fcp.color;
                break;
            case 4:
                button4.GetComponent<Image>().color = fcp.color;
                break;
            case 5:
                button5.GetComponent<Image>().color = fcp.color;
                break;
        }
        index++;
        if (index == 6) index = 1;
    }

    public void savedColorSelect(GameObject button){
        savedColorSelected = true;
        selectedColor = button.GetComponent<Image>().color;
        colorButton.GetComponent<Image>().color = selectedColor;
    }

    public void increaseWidth(){
        if(selectedWidth < 0.02f) {
            selectedWidth += 0.001f;
            colorButton.transform.localScale += scaleChange;
        }
    }

    public void decreaseWidth(){
        if(selectedWidth > 0.001f) {
            selectedWidth -= 0.001f;
            colorButton.transform.localScale -= scaleChange;
        }
    }

    public Color getSelectedColor(){
        return selectedColor;
    }

    public float getSelectedWidth(){
        return selectedWidth;
    }

    public void handleToolButton(){
        if(drawMode){
            drawMode = false;
            //paletteButton.GetComponent<Image>().color = inactiveColor;

        } else {

        }
    }

    public void clicked(string button) {
    ARDebugManager.Instance.LogInfo($"{button} button clicked!");
    switch(button)
    {
        case "openColorModal":
            if(modalWindow.activeSelf){
                modalWindow.SetActive(false);
                toolsButton.GetComponent<Button>().interactable = (true);                
                toolsButton.GetComponent<Image>().color = activeColor;
                background1.SetActive(false);
                drawMode = true;
            } else {
                modalWindow.SetActive(true);
                toolsButton.GetComponent<Button>().interactable = (false);
                toolsButton.GetComponent<Image>().color = inactiveColor;
                background1.SetActive(true);
                drawMode = false;
            }
            
            break;
        case "closeColorModal":
            modalWindow.SetActive(false);
            toolsButton.GetComponent<Button>().interactable = (true);
            toolsButton.GetComponent<Image>().color = activeColor;
            drawMode = true;
            background1.SetActive(false);
            break;
        case "openFurtherToolsPanel":
            secondPanel.SetActive(true);
            drawMode = false;
            selectMode = true;
            break;
        case "closeFurtherToolsPanel":
            secondPanel.SetActive(false);
            drawMode = true;
            selectMode = false;
            dragMode = false;
            background2.SetActive(false);
            break;
        case "dragMode":
            if(dragMode){
                dragMode = false;
                selectMode = true;
                background2.SetActive(false);
            } else {
                dragMode = true;
                selectMode = false;
                background2.SetActive(true);
            }
            break;
        
    }
    
    }
}
