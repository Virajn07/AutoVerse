using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AutoVerse
{
    public class AutoVerseController : MonoBehaviour
    {
        public Transform car;
        public Transform leftDoor;
        public Transform rightDoor;
        public Light[] headlights;
        public Text statusText;
        public Text modelText;
        public Text specificationsText;

        private bool doorsOpen;
        private bool lightsOn;
        private bool rotating;
        private float rotateDirection = 1f;

        // The scene builder creates its buttons in the Editor. Runtime listeners
        // are not serialized into a scene, so reconnect each button when the
        // scene starts.
        void Awake()
        {
            foreach (Button button in FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                switch (button.name)
                {
                    case "ENTER SHOWROOM":
                    case "Go SHOWROOM":
                    case "Nav SHOWROOM": button.onClick.AddListener(GoToShowroom); break;
                    case "Go SUV ZONE":
                    case "Nav SUV": button.onClick.AddListener(GoToSUVZone); break;
                    case "Go EV ZONE":
                    case "Nav EV": button.onClick.AddListener(GoToEVZone); break;
                    case "Go LUXURY CAR":
                    case "Nav LUXURY CAR":
                    case "Go DESIGN STUDIO":
                    case "Nav STUDIO": button.onClick.AddListener(GoToStudio); break;
                    case "Home": button.onClick.AddListener(GoHome); break;
                    case "Rotate Left": button.onClick.AddListener(RotateLeft); break;
                    case "Pause Rotation": button.onClick.AddListener(StopRotation); break;
                    case "Rotate Right": button.onClick.AddListener(RotateRight); break;
                    case "Door Control": button.onClick.AddListener(ToggleDoors); break;
                    case "Headlight Control": button.onClick.AddListener(ToggleLights); break;
                }
            }
        }

        public void GoToShowroom() => LoadScene("MainShowroom");
        public void GoToSUVZone() => LoadScene("SUVZone");
        public void GoToEVZone() => LoadScene("EVZone");
        public void GoToStudio() => LoadScene("CustomizationStudio");
        public void GoHome() => LoadScene("MainMenu");

        void Update()
        {
            if (rotating && car) car.Rotate(Vector3.up, 32f * rotateDirection * Time.deltaTime, Space.World);
            if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())) InspectClickedObject();
        }

        public void LoadScene(string sceneName)
        {
            if (statusText) statusText.text = "Loading " + sceneName.Replace("_", " ") + "...";
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            yield return new WaitForSecondsRealtime(0.12f);
            SceneManager.LoadSceneAsync(sceneName);
        }

        public void RotateLeft() { rotating = true; rotateDirection = -1f; SetStatus("Drag the car with the rotation controls."); }
        public void RotateRight() { rotating = true; rotateDirection = 1f; SetStatus("Drag the car with the rotation controls."); }
        public void StopRotation() { rotating = false; SetStatus("Rotation paused."); }

        public void ToggleDoors()
        {
            if (!leftDoor && !rightDoor)
            {
                SetStatus("This model has no separate animated door parts.");
                return;
            }
            doorsOpen = !doorsOpen;
            if (leftDoor) StartCoroutine(AnimateDoor(leftDoor, doorsOpen ? -58f : 0f));
            if (rightDoor) StartCoroutine(AnimateDoor(rightDoor, doorsOpen ? 58f : 0f));
            SetStatus(doorsOpen ? "Doors opened." : "Doors closed.");
        }

        private IEnumerator AnimateDoor(Transform door, float targetAngle)
        {
            Quaternion start = door.localRotation;
            Quaternion end = Quaternion.Euler(0f, targetAngle, 0f);
            float t = 0f;
            while (t < 1f) { t += Time.deltaTime * 2.5f; door.localRotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0f,1f,t)); yield return null; }
        }

        public void ToggleLights()
        {
            lightsOn = !lightsOn;
            foreach (Light lamp in headlights) if (lamp) lamp.enabled = lightsOn;
            SetStatus(lightsOn ? "Headlights on." : "Headlights off.");
        }

        private void InspectClickedObject()
        {
            if (!Camera.main || !car) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;
            Transform candidate = hit.transform;
            while (candidate != null && !candidate.name.StartsWith("Car - ")) candidate = candidate.parent;
            if (candidate == null) return;

            car = candidate;
            leftDoor = car.Find("Left_Door");
            rightDoor = car.Find("Right_Door");
            headlights = car.GetComponentsInChildren<Light>();
            if (leftDoor) leftDoor.localRotation=Quaternion.identity;
            if (rightDoor) rightDoor.localRotation=Quaternion.identity;
            foreach (Light lamp in headlights) if (lamp) lamp.enabled=false;
            doorsOpen = false;
            lightsOn = false;
            string model = car.name.Replace("Car - ", "");
            if (modelText) modelText.text = model;
            if (specificationsText)
            {
                if (model.Contains("VOLT")) specificationsText.text = "ALL ELECTRIC  •  520 KM RANGE\nFast charge ready  /  0–100 in 4.8 sec";
                else if (model.Contains("TERRA")) specificationsText.text = "ALL TERRAIN SUV\nAdventure package  /  5 seat comfort";
                else specificationsText.text = "PREMIUM PERFORMANCE\nSport package  /  Interactive concept model";
            }
            SetStatus("Selected " + model + "  •  " + hit.collider.name.Replace("_", " "));
        }

        private void SetStatus(string message) { if (statusText) statusText.text = message; }
    }
}
