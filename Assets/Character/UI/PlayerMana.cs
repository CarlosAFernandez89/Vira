using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Character.UI
{
    [UxmlElement]
    public partial class PlayerMana : VisualElement
    {
        static readonly CustomStyleProperty<Color> SFillColorProperty = new CustomStyleProperty<Color>("--fill-color");
        static readonly CustomStyleProperty<Color> SBackgroundColorProperty = new CustomStyleProperty<Color>("--background-color");
        
        Color _fillColor;
        Color _backgroundColor;
        
        [SerializeField, DontCreateProperty] private float _progress;

        [UxmlAttribute, CreateProperty] [Range(1f, 5f)] public float LerpSpeedMultiplier;

        [UxmlAttribute, CreateProperty]
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = Mathf.Lerp(_progress, Mathf.Clamp(value, 0.0001f, 1f),Time.deltaTime * LerpSpeedMultiplier);
                MarkDirtyRepaint();
            }
        }
        public PlayerMana()
        {
            RegisterCallback<CustomStyleResolvedEvent>(CustomStyleResolved);
            generateVisualContent += GenerateVisualContent;
        }

        private void CustomStyleResolved(CustomStyleResolvedEvent e)
        {
            if (e.currentTarget == this)
            {
                PlayerMana element = (PlayerMana)e.currentTarget;
                element.UpdateCustomStyles();
            }
        }

        void UpdateCustomStyles()
        {
            bool repaint = 
                customStyle.TryGetValue(SFillColorProperty, out _fillColor) 
                || customStyle.TryGetValue(SBackgroundColorProperty, out _backgroundColor);

            if (repaint)
            {
                MarkDirtyRepaint();
            }
        }

        private void GenerateVisualContent(MeshGenerationContext context)
        {
            float width = contentRect.width;
            float height = contentRect.height;

            var painter = context.painter2D;
            painter.BeginPath();
            painter.lineWidth = 10f;
            painter.Arc(new Vector2(width * 0.5f, height), width * 0.5f, 180f, 0f);
            painter.ClosePath();
            painter.fillColor = _backgroundColor;
            painter.Fill(FillRule.NonZero);
            painter.Stroke();
            
            //Fill
            painter.BeginPath();
            painter.LineTo(new Vector2(width * 0.5f, height));
            painter.lineWidth = 10f;
            
            float amount = 180f * ((1f-Progress)/1f);
            
            painter.Arc(new Vector2(width * 0.5f, height), width * 0.5f, 180f, 0f - amount);
            painter.ClosePath();
            painter.fillColor = _fillColor;
            painter.Fill();
            painter.Stroke();
        }
    }
}
