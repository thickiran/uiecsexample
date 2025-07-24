using UnityEngine;
using UnityEngine.UI;

public class DragPreview : MonoBehaviour
    {
        [Header("Preview Settings")]
        public float followSpeed = 10f;
        public Vector2 offset = new Vector2(10, -10);
        public float fadeInDuration = 0.1f;
        public float fadeOutDuration = 0.2f;
        
        [Header("Visual Components")]
        public Image previewImage;
        public Text stackCountText;
        public CanvasGroup canvasGroup;
        
        private Camera uiCamera;
        private Canvas parentCanvas;
        private RectTransform rectTransform;
        private Coroutine fadeCoroutine;
        
        public static DragPreview Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeComponents();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void InitializeComponents()
        {
            rectTransform = GetComponent<RectTransform>();
            
            // Get or create canvas group
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Find UI camera
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                uiCamera = parentCanvas.worldCamera;
            }
            
            // Create preview image if it doesn't exist
            if (previewImage == null)
            {
                CreatePreviewImage();
            }
            
            // Create stack count text if it doesn't exist
            if (stackCountText == null)
            {
                CreateStackCountText();
            }
            
            // Start hidden
            gameObject.SetActive(false);
        }
        
        void CreatePreviewImage()
        {
            GameObject imageObj = new GameObject("PreviewImage");
            imageObj.transform.SetParent(transform, false);
            
            previewImage = imageObj.AddComponent<Image>();
            previewImage.raycastTarget = false;
            
            var imageRect = previewImage.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
        }
        
        void CreateStackCountText()
        {
            GameObject textObj = new GameObject("StackCount");
            textObj.transform.SetParent(transform, false);
            
            stackCountText = textObj.AddComponent<Text>();
            stackCountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            stackCountText.fontSize = 12;
            stackCountText.color = Color.white;
            stackCountText.alignment = TextAnchor.LowerRight;
            stackCountText.raycastTarget = false;
            
            var textRect = stackCountText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0);
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = new Vector2(-2, -2);
        }
        
        public void ShowPreview(Sprite itemSprite, int stackCount = 1)
        {
            if (previewImage != null)
            {
                previewImage.sprite = itemSprite;
                previewImage.color = Color.white;
            }
            
            if (stackCountText != null)
            {
                if (stackCount > 1)
                {
                    stackCountText.text = stackCount.ToString();
                    stackCountText.gameObject.SetActive(true);
                }
                else
                {
                    stackCountText.gameObject.SetActive(false);
                }
            }
            
            gameObject.SetActive(true);
            
            // Fade in
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        
        public void HidePreview()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        
        public void UpdatePosition(Vector2 screenPosition)
        {
            if (rectTransform == null) return;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPosition + offset,
                uiCamera,
                out localPoint);
            
            if (followSpeed > 0)
            {
                Vector2 targetPosition = localPoint;
                Vector2 currentPosition = rectTransform.localPosition;
                rectTransform.localPosition = Vector2.Lerp(currentPosition, targetPosition, 
                    followSpeed * Time.unscaledDeltaTime);
            }
            else
            {
                rectTransform.localPosition = localPoint;
            }
        }
        
        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
                
                yield return null;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
                
                yield return null;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            
            gameObject.SetActive(false);
        }
        
        public void SetPreviewSize(Vector2 size)
        {
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = size;
            }
        }
        
        public void SetPreviewAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }
        
        void Update()
        {
            // Optional: Follow mouse cursor smoothly
            if (gameObject.activeInHierarchy && Input.mousePosition != Vector3.zero)
            {
                UpdatePosition(Input.mousePosition);
            }
        }
    }