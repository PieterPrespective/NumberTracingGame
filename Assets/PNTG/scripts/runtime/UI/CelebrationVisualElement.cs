using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PNTG
{
    /// <summary>
    /// Visual element that displays celebration animations (fireworks and balloons) using Painter2D
    /// </summary>
    public class CelebrationVisualElement : VisualElement
    {
        private List<Firework> fireworks = new List<Firework>();
        private List<Balloon> balloons = new List<Balloon>();
        private List<PopEffect> popEffects = new List<PopEffect>();
        private float animationTime = 0f;
        private bool isAnimating = false;
        private float nextFireworkTime = 0f;
        private float nextBalloonTime = 0f;
        private IVisualElementScheduledItem updateTask;
        
        private class Firework
        {
            public Vector2 position;
            public Vector2 velocity;
            public float lifetime;
            public float maxLifetime;
            public bool exploded;
            public List<Particle> particles;
            public Color color;
            
            public class Particle
            {
                public Vector2 position;
                public Vector2 velocity;
                public float lifetime;
                public Color color;
                
                public Particle(Vector2 pos, Vector2 vel, Color col)
                {
                    position = pos;
                    velocity = vel;
                    color = col;
                    lifetime = 1f;
                }
            }
            
            public Firework(Vector2 startPos, Vector2 targetPos, Color col)
            {
                position = startPos;
                Vector2 direction = (targetPos - startPos).normalized;
                float speed = Random.Range(300f, 500f);
                velocity = direction * speed;
                lifetime = 0f;
                maxLifetime = Vector2.Distance(startPos, targetPos) / speed;
                exploded = false;
                particles = new List<Particle>();
                color = col;
            }
            
            public void Explode()
            {
                exploded = true;
                int particleCount = Random.Range(20, 40);
                
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = (360f / particleCount) * i + Random.Range(-10f, 10f);
                    float speed = Random.Range(100f, 300f);
                    Vector2 vel = new Vector2(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * speed,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * speed
                    );
                    
                    // Vary color slightly for each particle
                    Color particleColor = color;
                    particleColor.r = Mathf.Clamp01(particleColor.r + Random.Range(-0.2f, 0.2f));
                    particleColor.g = Mathf.Clamp01(particleColor.g + Random.Range(-0.2f, 0.2f));
                    particleColor.b = Mathf.Clamp01(particleColor.b + Random.Range(-0.2f, 0.2f));
                    
                    particles.Add(new Particle(position, vel, particleColor));
                }
            }
        }
        
        private class Balloon
        {
            public Vector2 position;
            public Vector2 velocity;
            public float wobblePhase;
            public float wobbleSpeed;
            public float wobbleAmplitude;
            public float size;
            public Color color;
            public float lifetime;
            
            public Balloon(float x, float screenHeight)
            {
                position = new Vector2(x, screenHeight + 50f);
                velocity = new Vector2(0, Random.Range(-300f, -500f)); // Much faster upward movement
                wobblePhase = Random.Range(0f, Mathf.PI * 2f);
                wobbleSpeed = Random.Range(2f, 4f);
                wobbleAmplitude = Random.Range(20f, 40f);
                size = Random.Range(30f, 60f);
                
                // Random bright colors
                float hue = Random.Range(0f, 1f);
                color = Color.HSVToRGB(hue, 0.8f, 1f);
                lifetime = 0f;
            }
            
            public void Update(float deltaTime)
            {
                position += velocity * deltaTime;
                wobblePhase += wobbleSpeed * deltaTime;
                lifetime += deltaTime;
            }
            
            public float GetWobbleOffset()
            {
                return Mathf.Sin(wobblePhase) * wobbleAmplitude;
            }
            
            public bool ContainsPoint(Vector2 point)
            {
                float wobbleX = GetWobbleOffset();
                Vector2 balloonCenter = position + new Vector2(wobbleX, 0);
                return Vector2.Distance(point, balloonCenter) <= size;
            }
        }
        
        private class PopEffect
        {
            public Vector2 position;
            public float lifetime;
            public float maxLifetime;
            public Color color;
            public List<PopParticle> particles;
            
            public class PopParticle
            {
                public Vector2 position;
                public Vector2 velocity;
                public float lifetime;
                public Color color;
                
                public PopParticle(Vector2 pos, Vector2 vel, Color col)
                {
                    position = pos;
                    velocity = vel;
                    color = col;
                    lifetime = 1f;
                }
            }
            
            public PopEffect(Vector2 pos, Color balloonColor)
            {
                position = pos;
                lifetime = 0f;
                maxLifetime = 1f;
                color = balloonColor;
                particles = new List<PopParticle>();
                
                // Create pop particles
                int particleCount = Random.Range(8, 15);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = Random.Range(0f, 360f);
                    float speed = Random.Range(50f, 150f);
                    Vector2 vel = new Vector2(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * speed,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * speed
                    );
                    
                    particles.Add(new PopParticle(pos, vel, balloonColor));
                }
            }
        }
        
        public CelebrationVisualElement()
        {
            generateVisualContent += OnGenerateVisualContent;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            
            // Enable pointer events for balloon popping
            pickingMode = PickingMode.Position;
            
            // Register for pointer events
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!isAnimating) return;
            
            Vector2 localPosition = evt.localPosition;
            
            // Check if any balloon was clicked/touched
            for (int i = balloons.Count - 1; i >= 0; i--)
            {
                var balloon = balloons[i];
                if (balloon.ContainsPoint(localPosition))
                {
                    // Create pop effect
                    float wobbleX = balloon.GetWobbleOffset();
                    Vector2 popPosition = balloon.position + new Vector2(wobbleX, 0);
                    popEffects.Add(new PopEffect(popPosition, balloon.color));
                    
                    // Remove the balloon
                    balloons.RemoveAt(i);
                    
                    // Consume the event so it doesn't propagate
                    evt.StopPropagation();
                    break;
                }
            }
        }
        
        public void StartCelebration()
        {
            // Stop any existing animation task
            if (updateTask != null)
            {
                updateTask.Pause();
            }
            
            isAnimating = true;
            animationTime = 0f;
            nextFireworkTime = 0f;
            nextBalloonTime = 0f;
            fireworks.Clear();
            balloons.Clear();
            popEffects.Clear();
            
            // Schedule regular updates
            updateTask = schedule.Execute(UpdateAnimation).Every(16); // ~60 FPS
        }
        
        public void StopCelebration()
        {
            isAnimating = false;
            
            // Stop the animation task
            if (updateTask != null)
            {
                updateTask.Pause();
                updateTask = null;
            }
            
            fireworks.Clear();
            balloons.Clear();
            popEffects.Clear();
            MarkDirtyRepaint();
        }
        
        private void UpdateAnimation()
        {
            if (!isAnimating)
            {
                if (updateTask != null)
                {
                    updateTask.Pause();
                    updateTask = null;
                }
                return;
            }
            
            float deltaTime = 0.016f; // Assuming 60 FPS
            animationTime += deltaTime;
            
            // Spawn new fireworks
            if (animationTime >= nextFireworkTime)
            {
                SpawnFirework();
                nextFireworkTime = animationTime + Random.Range(0.2f, 0.5f);
            }
            
            // Spawn new balloons
            if (animationTime >= nextBalloonTime)
            {
                SpawnBalloon();
                nextBalloonTime = animationTime + Random.Range(0.3f, 0.8f);
            }
            
            // Update fireworks
            for (int i = fireworks.Count - 1; i >= 0; i--)
            {
                var firework = fireworks[i];
                firework.lifetime += deltaTime;
                
                if (!firework.exploded)
                {
                    firework.position += firework.velocity * deltaTime;
                    firework.velocity.y += 300f * deltaTime; // Gravity
                    
                    if (firework.lifetime >= firework.maxLifetime)
                    {
                        firework.Explode();
                    }
                }
                else
                {
                    // Update particles
                    for (int j = firework.particles.Count - 1; j >= 0; j--)
                    {
                        var particle = firework.particles[j];
                        particle.position += particle.velocity * deltaTime;
                        particle.velocity.y += 200f * deltaTime; // Gravity
                        particle.lifetime -= deltaTime * 2f; // Fade out
                        
                        if (particle.lifetime <= 0)
                        {
                            firework.particles.RemoveAt(j);
                        }
                    }
                    
                    // Remove firework if all particles are gone
                    if (firework.particles.Count == 0)
                    {
                        fireworks.RemoveAt(i);
                    }
                }
            }
            
            // Update balloons
            for (int i = balloons.Count - 1; i >= 0; i--)
            {
                var balloon = balloons[i];
                balloon.Update(deltaTime);
                
                // Remove balloons that have floated off screen
                if (balloon.position.y < -100f)
                {
                    balloons.RemoveAt(i);
                }
            }
            
            // Update pop effects
            for (int i = popEffects.Count - 1; i >= 0; i--)
            {
                var popEffect = popEffects[i];
                popEffect.lifetime += deltaTime;
                
                // Update pop particles
                for (int j = popEffect.particles.Count - 1; j >= 0; j--)
                {
                    var particle = popEffect.particles[j];
                    particle.position += particle.velocity * deltaTime;
                    particle.velocity.y += 150f * deltaTime; // Gravity
                    particle.lifetime -= deltaTime * 3f; // Fade out faster
                    
                    if (particle.lifetime <= 0)
                    {
                        popEffect.particles.RemoveAt(j);
                    }
                }
                
                // Remove pop effect if all particles are gone
                if (popEffect.particles.Count == 0)
                {
                    popEffects.RemoveAt(i);
                }
            }
            
            // Stop animation after 10 seconds
            if (animationTime > 10f)
            {
                StopCelebration();
            }
            
            MarkDirtyRepaint();
        }
        
        private void SpawnFirework()
        {
            float x = Random.Range(100f, resolvedStyle.width - 100f);
            float y = resolvedStyle.height - 50f;
            Vector2 startPos = new Vector2(x, y);
            
            float targetX = Random.Range(100f, resolvedStyle.width - 100f);
            float targetY = Random.Range(100f, resolvedStyle.height * 0.5f);
            Vector2 targetPos = new Vector2(targetX, targetY);
            
            // Random bright color
            float hue = Random.Range(0f, 1f);
            Color color = Color.HSVToRGB(hue, 0.9f, 1f);
            
            fireworks.Add(new Firework(startPos, targetPos, color));
        }
        
        private void SpawnBalloon()
        {
            float x = Random.Range(50f, resolvedStyle.width - 50f);
            balloons.Add(new Balloon(x, resolvedStyle.height));
        }
        
        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            
            // Draw balloons (behind fireworks)
            foreach (var balloon in balloons)
            {
                DrawBalloon(painter, balloon);
            }
            
            // Draw pop effects
            foreach (var popEffect in popEffects)
            {
                DrawPopEffect(painter, popEffect);
            }
            
            // Draw fireworks
            foreach (var firework in fireworks)
            {
                if (!firework.exploded)
                {
                    DrawFireworkTrail(painter, firework);
                }
                else
                {
                    DrawFireworkExplosion(painter, firework);
                }
            }
        }
        
        private void DrawBalloon(Painter2D painter, Balloon balloon)
        {
            float wobbleX = balloon.GetWobbleOffset();
            Vector2 pos = balloon.position + new Vector2(wobbleX, 0);
            
            // Balloon body
            painter.fillColor = balloon.color;
            painter.strokeColor = Color.clear;
            
            painter.BeginPath();
            painter.Arc(pos, balloon.size, 0, 360);
            painter.Fill();
            
            // Highlight
            Color highlight = Color.white;
            highlight.a = 0.3f;
            painter.fillColor = highlight;
            
            painter.BeginPath();
            painter.Arc(pos + new Vector2(-balloon.size * 0.3f, -balloon.size * 0.3f), 
                       balloon.size * 0.3f, 0, 360);
            painter.Fill();
            
            // String
            painter.strokeColor = Color.gray;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.MoveTo(pos + new Vector2(0, balloon.size));
            
            // Wavy string
            float stringLength = balloon.size * 2f;
            int segments = 10;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float stringWobble = Mathf.Sin((balloon.wobblePhase + t * 4f)) * 5f;
                Vector2 stringPos = pos + new Vector2(stringWobble, balloon.size + t * stringLength);
                painter.LineTo(stringPos);
            }
            painter.Stroke();
        }
        
        private void DrawFireworkTrail(Painter2D painter, Firework firework)
        {
            // Draw glowing trail
            painter.strokeColor = Color.clear;
            
            // Multiple layers for glow effect
            for (int i = 0; i < 3; i++)
            {
                float alpha = 0.3f - i * 0.1f;
                Color glowColor = firework.color;
                glowColor.a = alpha;
                painter.fillColor = glowColor;
                
                float size = 8f - i * 2f;
                painter.BeginPath();
                painter.Arc(firework.position, size, 0, 360);
                painter.Fill();
            }
            
            // Core
            painter.fillColor = Color.white;
            painter.BeginPath();
            painter.Arc(firework.position, 3f, 0, 360);
            painter.Fill();
        }
        
        private void DrawFireworkExplosion(Painter2D painter, Firework firework)
        {
            foreach (var particle in firework.particles)
            {
                Color color = particle.color;
                color.a = particle.lifetime;
                painter.fillColor = color;
                painter.strokeColor = Color.clear;
                
                // Particle with glow
                painter.BeginPath();
                painter.Arc(particle.position, 4f * particle.lifetime, 0, 360);
                painter.Fill();
                
                // Inner bright core
                Color coreColor = Color.white;
                coreColor.a = particle.lifetime * 0.8f;
                painter.fillColor = coreColor;
                painter.BeginPath();
                painter.Arc(particle.position, 2f * particle.lifetime, 0, 360);
                painter.Fill();
            }
        }
        
        private void DrawPopEffect(Painter2D painter, PopEffect popEffect)
        {
            foreach (var particle in popEffect.particles)
            {
                Color color = particle.color;
                color.a = particle.lifetime * 0.8f;
                painter.fillColor = color;
                painter.strokeColor = Color.clear;
                
                // Small colorful particles bursting outward
                painter.BeginPath();
                painter.Arc(particle.position, 3f * particle.lifetime, 0, 360);
                painter.Fill();
            }
        }
    }
}