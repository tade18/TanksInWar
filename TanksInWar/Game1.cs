using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TanksInWar;

public class Game1 : Game
{
    Texture2D _tankTexture;
    Texture2D _backgroundTexture;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _projectileTexture;
    private Vector2 _tankPosition;
    private float _tankRotation;
    private const float TankSpeed = 110f;
    private const float RotationSpeed = 3f;
    private const float ProjectileSpeed = 500f;
    private const float ProjectileLifetime = 1.5f;

    private List<Projectile> _projectiles;
    private bool _canShoot = true;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _tankPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        _projectiles = new List<Projectile>();
        base.Initialize();
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 768;
        _graphics.PreferredBackBufferHeight = 432;
        _graphics.ApplyChanges();

    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _tankTexture = Content.Load<Texture2D>("tank");
        _backgroundTexture = Content.Load<Texture2D>("tankBackground");

        _projectileTexture = new Texture2D(GraphicsDevice, 5, 5);
        Color[] projectileData = new Color[5 * 5];
        for (int i = 0; i < projectileData.Length; ++i) projectileData[i] = Color.Red;
        _projectileTexture.SetData(projectileData);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        var keyboardState = Keyboard.GetState();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (keyboardState.IsKeyDown(Keys.A))
            _tankRotation -= RotationSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            _tankRotation += RotationSpeed * deltaTime;
        
        Vector2 direction = new Vector2((float)Math.Cos(_tankRotation), (float)Math.Sin(_tankRotation));

        if (keyboardState.IsKeyDown(Keys.W))
            _tankPosition += direction * TankSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.S))
            _tankPosition -= direction * TankSpeed * deltaTime;
        
        _tankPosition.X = MathHelper.Clamp(_tankPosition.X, 0 + _tankTexture.Width/2, GraphicsDevice.Viewport.Width - _tankTexture.Width/2);
        _tankPosition.Y = MathHelper.Clamp(_tankPosition.Y, 0 + _tankTexture.Height, GraphicsDevice.Viewport.Height - _tankTexture.Height);
        
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed && _canShoot)
        {
            ShootProjectile();
            _canShoot = false;
        }
        else if (mouseState.LeftButton == ButtonState.Released)
        {
            _canShoot = true;
        }
        
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            _projectiles[i].Update(deltaTime);
            
            if (_projectiles[i].LifeTime > ProjectileLifetime)
            {
                _projectiles.RemoveAt(i);
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.BurlyWood);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_backgroundTexture, new Vector2(0,0), Color.White);
        _spriteBatch.Draw(_tankTexture, _tankPosition, null, Color.White, _tankRotation, new Vector2(25, 15),
            Vector2.One, SpriteEffects.None, 0f);

        foreach (var projectile in _projectiles)
        {
            projectile.Draw(_spriteBatch);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void ShootProjectile()
    {
        Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        Vector2 projectileDirection = Vector2.Normalize(mousePosition - _tankPosition);

        Projectile newProjectile = new Projectile(_projectileTexture, _tankPosition, projectileDirection, ProjectileSpeed);
        _projectiles.Add(newProjectile);
    }

    public class Projectile
    {
        private Texture2D _texture;
        private Vector2 _position;
        private Vector2 _direction;
        private float _speed;
        public float LifeTime { get; private set; }

        public Projectile(Texture2D texture, Vector2 position, Vector2 direction, float speed)
        {
            _texture = texture;
            _position = position;
            _direction = direction;
            _speed = speed;
            LifeTime = 0f;
        }

        public void Update(float deltaTime)
        {
            _position += _direction * _speed * deltaTime;
            LifeTime += deltaTime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, Color.White);
        }
    }
}