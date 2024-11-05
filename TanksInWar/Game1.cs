using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace TanksInWar;

public class Game1 : Game
{
    Texture2D _tankTexture;
    Texture2D _tankTowerTexture;
    Texture2D _backgroundTexture;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    
    private Texture2D _enemyTankTexture;
    private EnemyTank _enemyTank;
    private Texture2D _enemyTurretTexture;
    

    Song _backgroundMusic;

    private Texture2D _projectileTexture;
    private Vector2 _tankPosition;
    private float _tankRotation;
    private float _turretRotation;
    private const float TankSpeed = 110f;
    private const float RotationSpeed = 3f;
    private const float ProjectileSpeed = 900f;
    private const float ProjectileLifetime = 0.85f;
    private float _turretLength = 22f;

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
        
        _enemyTankTexture = Content.Load<Texture2D>("tank");
        _enemyTank = new EnemyTank(_enemyTankTexture, new Vector2(300, 300));
        _enemyTurretTexture = Content.Load<Texture2D>("tank_tower");
        
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _tankTexture = Content.Load<Texture2D>("tank");
        _tankTowerTexture = Content.Load<Texture2D>("tank_tower");
        _backgroundTexture = Content.Load<Texture2D>("tankBackground");
        
        _backgroundMusic = Content.Load<Song>("ambientBackground");
        MediaPlayer.Play(_backgroundMusic);
        MediaPlayer.IsRepeating = true;

        _projectileTexture = new Texture2D(GraphicsDevice, 4, 4);
        Color[] projectileData = new Color[4 * 4];
        for (int i = 0; i < projectileData.Length; ++i) projectileData[i] = Color.DarkOrange;
        _projectileTexture.SetData(projectileData);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        Vector2 turretDirection = mousePosition - _tankPosition;
        _turretRotation = (float)Math.Atan2(turretDirection.Y, turretDirection.X);
        
        var keyboardState = Keyboard.GetState();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (keyboardState.IsKeyDown(Keys.A))
            _tankRotation -= RotationSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            _tankRotation += RotationSpeed * deltaTime;
        
        Vector2 direction = new Vector2((float)Math.Cos(_tankRotation), (float)Math.Sin(_tankRotation));
        
        
        _enemyTank.Update(gameTime, _tankPosition, _projectiles, _projectileTexture, Window.ClientBounds.Width, Window.ClientBounds.Height);
        

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
        _spriteBatch.Draw(_tankTexture, _tankPosition, null, Color.White, _tankRotation, new Vector2(_tankTexture.Width / 2, _tankTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        foreach (var projectile in _projectiles)
        {
            projectile.Draw(_spriteBatch);
        }
        Vector2 _turretOffset = new Vector2(-5, 0);
        Vector2 turretPosition = _tankPosition + Vector2.Transform(_turretOffset, Matrix.CreateRotationZ(_tankRotation));
        _spriteBatch.Draw(_tankTowerTexture, turretPosition, null, Color.White, _turretRotation, new Vector2(_tankTowerTexture.Width / 2 -10, _tankTowerTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        _enemyTank.Draw(_spriteBatch, _enemyTurretTexture);
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void ShootProjectile()
    {
        Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        Vector2 projectileDirection = Vector2.Normalize(mousePosition - _tankPosition);
        Vector2 turretEndPosition = _tankPosition + Vector2.Transform(new Vector2(_turretLength, 0), Matrix.CreateRotationZ(_turretRotation));
        Projectile newProjectile = new Projectile(_projectileTexture, turretEndPosition, projectileDirection, ProjectileSpeed);
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
    public class EnemyTank
{
    
    private Texture2D _texture;
    private Vector2 _position;
    private float _rotation;
    private float _speed;
    private float _shootCooldown;
    private Random _random;
    private float _turretRotation;

    public EnemyTank(Texture2D texture, Vector2 startPosition)
    {
        _texture = texture;
        _position = startPosition;
        _rotation = 0f;
        _speed = 1f; 
        _shootCooldown = 0f;
        _random = new Random();
    }


    public void Update(GameTime gameTime, Vector2 playerPosition, List<Projectile> projectiles, Texture2D projectileTexture, int windowWidth, int windowHeight)    {

        Vector2 directionToPlayer = playerPosition - _position;
        _turretRotation = (float)Math.Atan2(directionToPlayer.Y, directionToPlayer.X);
        if (_random.NextDouble() < 0.01)
        {
            _rotation = (float)(_random.NextDouble() * Math.PI * 2);
        }
        
        Vector2 direction = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));
        _position += direction * _speed;
        if (_position.X < 10 || _position.Y < 10 )
        {
            _position.X +=5;
            _position.Y += 5;
            _rotation += (float)Math.PI; 
            _position += direction * _speed; 
        }

        if (_position.X > windowWidth-10 || _position.Y > windowHeight-10)
        {
            _position.X -=5;
            _position.Y -= 5;
            _rotation += (float)Math.PI; 
            _position += direction * _speed; 
        }
        _shootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_shootCooldown <= 0)
        {
            Shoot(playerPosition, projectiles, projectileTexture);
            _shootCooldown = 2f;
        }
    }

    private void Shoot(Vector2 playerPosition, List<Projectile> projectiles, Texture2D projectileTexture)
    {
        Vector2 direction = Vector2.Normalize(playerPosition - _position);
        Vector2 turretEndPosition = _position + direction * 20f;

        Projectile newProjectile = new Projectile(projectileTexture, turretEndPosition, direction, 900f);
        projectiles.Add(newProjectile);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D turretTexture)
    {
        Vector2 _turretOffset = new Vector2(-5, 0);
        Vector2 turretPosition = _position + Vector2.Transform(_turretOffset, Matrix.CreateRotationZ(_rotation));
        spriteBatch.Draw(_texture, _position, null, Color.Peru, _rotation, new Vector2(_texture.Width / 2, _texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        spriteBatch.Draw(turretTexture, turretPosition, null, Color.Peru, _turretRotation, new Vector2(turretTexture.Width / 2 -10, turretTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
    }
}
}