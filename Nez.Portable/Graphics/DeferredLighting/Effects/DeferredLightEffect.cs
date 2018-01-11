using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.ECS;
using Nez.Graphics.DeferredLighting.Lights;
using Nez.Graphics.Effects;

namespace Nez.Graphics.DeferredLighting.Effects
{
    public class DeferredLightEffect : Effect
    {
        private readonly EffectPass _areaLightPass;
        private readonly EffectPass _clearGBufferPass;
        private readonly EffectPass _directionalLightPass;
        private readonly EffectPass _finalCombinePass;
        private readonly EffectPass _pointLightPass;
        private readonly EffectPass _spotLightPass;


        public DeferredLightEffect() : base(Core.GraphicsDevice, EffectResource.DeferredLightBytes)
        {
            _clearGBufferPass = Techniques["ClearGBuffer"].Passes[0];
            _pointLightPass = Techniques["DeferredPointLight"].Passes[0];
            _spotLightPass = Techniques["DeferredSpotLight"].Passes[0];
            _areaLightPass = Techniques["DeferredAreaLight"].Passes[0];
            _directionalLightPass = Techniques["DeferredDirectionalLight"].Passes[0];
            _finalCombinePass = Techniques["FinalCombine"].Passes[0];

            CacheEffectParameters();
        }


        private void CacheEffectParameters()
        {
            // gBuffer
            _clearColorParam = Parameters["_clearColor"];

            // matrices
            _objectToWorldParam = Parameters["_objectToWorld"];
            _worldToViewParam = Parameters["_worldToView"];
            _projectionParam = Parameters["_projection"];
            _screenToWorldParam = Parameters["_screenToWorld"];

            // common
            _normalMapParam = Parameters["_normalMap"];
            _lightPositionParam = Parameters["_lightPosition"];
            _colorParam = Parameters["_color"];
            _lightRadiusParam = Parameters["_lightRadius"];
            _lightIntensityParam = Parameters["_lightIntensity"];

            // spot
            _lightDirectionParam = Parameters["_lightDirection"];
            _coneAngleParam = Parameters["_coneAngle"];

            // directional
            _specularIntensityParam = Parameters["_specularIntensity"];
            _specularPowerParam = Parameters["_specularPower"];
            _dirAreaLightDirectionParam = Parameters["_dirAreaLightDirection"];

            // final combine
            _ambientColorParam = Parameters["_ambientColor"];
            _colorMapParam = Parameters["_colorMap"];
            _lightMapParam = Parameters["_lightMap"];
        }


        public void PrepareClearGBuffer()
        {
            _clearGBufferPass.Apply();
        }


	    /// <summary>
	    ///     updates the camera matrixes in the Effect
	    /// </summary>
	    /// <param name="camera">Camera.</param>
	    public void UpdateForCamera(Camera camera)
        {
            SetWorldToViewMatrix(camera.TransformMatrix);
            SetProjectionMatrix(camera.ProjectionMatrix);
            SetScreenToWorld(Matrix.Invert(camera.ViewProjectionMatrix));
        }


	    /// <summary>
	    ///     updates the shader values for the light and sets the appropriate CurrentTechnique
	    /// </summary>
	    /// <param name="light">Light.</param>
	    public void UpdateForLight(DeferredLight light)
        {
            // check SpotLight first because it is a subclass of PointLight!
            if (light is SpotLight)
                UpdateForLight(light as SpotLight);
            else if (light is PointLight)
                UpdateForLight(light as PointLight);
            else if (light is AreaLight)
                UpdateForLight(light as AreaLight);
            else if (light is DirLight)
                UpdateForLight(light as DirLight);
        }


	    /// <summary>
	    ///     updates the shader values for the light and sets the appropriate CurrentTechnique
	    /// </summary>
	    /// <param name="light">Light.</param>
	    public void UpdateForLight(PointLight light)
        {
            SetLightPosition(new Vector3(light.Entity.Transform.Position + light.LocalOffset, light.ZPosition));
            SetColor(light.Color);
            SetLightRadius(light.Radius * light.Entity.Transform.Scale.X);
            SetLightIntensity(light.Intensity);

            var objToWorld = Matrix.CreateScale(light.Radius * light.Entity.Transform.Scale.X) *
                             Matrix.CreateTranslation(light.Entity.Transform.Position.X + light.LocalOffset.X,
                                 light.Entity.Transform.Position.Y + light.LocalOffset.Y, 0);
            SetObjectToWorldMatrix(objToWorld);

            _pointLightPass.Apply();
        }


	    /// <summary>
	    ///     updates the shader values for the light and sets the appropriate CurrentTechnique
	    /// </summary>
	    /// <param name="light">Light.</param>
	    public void UpdateForLight(SpotLight light)
        {
            UpdateForLight(light as PointLight);
            SetSpotLightDirection(light.Direction);
            SetSpotConeAngle(light.ConeAngle);

            _spotLightPass.Apply();
        }


	    /// <summary>
	    ///     updates the shader values for the light and sets the appropriate CurrentTechnique
	    /// </summary>
	    /// <param name="light">Light.</param>
	    public void UpdateForLight(AreaLight light)
        {
            SetColor(light.Color);
            SetAreaDirectionalLightDirection(light.Direction);
            SetLightIntensity(light.Intensity);

            var objToWorld =
                Matrix.CreateScale(light.Bounds.Width * light.Entity.Transform.Scale.X,
                    light.Bounds.Height * light.Entity.Transform.Scale.Y, 1f) * Matrix.CreateTranslation(
                    light.Bounds.X - light.Bounds.Width * 0.5f, light.Bounds.Y - light.Bounds.Height * 0.5f, 0);
            SetObjectToWorldMatrix(objToWorld);

            _areaLightPass.Apply();
        }


	    /// <summary>
	    ///     updates the shader values for the light and sets the appropriate CurrentTechnique
	    /// </summary>
	    /// <param name="light">Light.</param>
	    public void UpdateForLight(DirLight light)
        {
            SetColor(light.Color);
            SetAreaDirectionalLightDirection(light.Direction);
            SetSpecularPower(light.SpecularPower);
            SetSpecularIntensity(light.SpecularIntensity);

            _directionalLightPass.Apply();
        }

        #region EffectParameter caches

        // gBuffer
        private EffectParameter _clearColorParam;

        // matrices
        private EffectParameter _objectToWorldParam;

        private EffectParameter _worldToViewParam;
        private EffectParameter _projectionParam;
        private EffectParameter _screenToWorldParam;

        // common
        private EffectParameter _normalMapParam;

        private EffectParameter _lightPositionParam;
        private EffectParameter _colorParam;
        private EffectParameter _lightRadiusParam;
        private EffectParameter _lightIntensityParam;

        // spot
        private EffectParameter _lightDirectionParam;

        private EffectParameter _coneAngleParam;

        // directional
        private EffectParameter _specularIntensityParam;

        private EffectParameter _specularPowerParam;
        private EffectParameter _dirAreaLightDirectionParam; // shared with area light

        // final combine
        private EffectParameter _ambientColorParam;

        private EffectParameter _colorMapParam;
        private EffectParameter _lightMapParam;

        #endregion


        #region Matrix properties

        public void SetClearColor(Color color)
        {
            _clearColorParam.SetValue(color.ToVector3());
        }


        public void SetObjectToWorldMatrix(Matrix objToWorld)
        {
            _objectToWorldParam.SetValue(objToWorld);
        }


        public void SetWorldToViewMatrix(Matrix worldToView)
        {
            _worldToViewParam.SetValue(worldToView);
        }


        public void SetProjectionMatrix(Matrix projection)
        {
            _projectionParam.SetValue(projection);
        }


	    /// <summary>
	    ///     inverse of Camera.getViewProjectionMatrix
	    /// </summary>
	    /// <param name="screenToWorld">screenToWorld.</param>
	    public void SetScreenToWorld(Matrix screenToWorld)
        {
            _screenToWorldParam.SetValue(screenToWorld);
        }

        #endregion


        #region Point/Spot common properties

        public void SetNormalMap(Texture2D normalMap)
        {
            _normalMapParam.SetValue(normalMap);
        }


        public void SetLightPosition(Vector3 lightPosition)
        {
            _lightPositionParam.SetValue(lightPosition);
        }


        public void SetColor(Color color)
        {
            _colorParam.SetValue(color.ToVector3());
        }


        public void SetLightRadius(float radius)
        {
            _lightRadiusParam.SetValue(radius);
        }


        public void SetLightIntensity(float intensity)
        {
            _lightIntensityParam.SetValue(intensity);
        }

        #endregion


        #region Spot properties

	    /// <summary>
	    ///     directly sets the light direction
	    /// </summary>
	    /// <param name="lightDirection">Light direction.</param>
	    public void SetSpotLightDirection(Vector2 lightDirection)
        {
            _lightDirectionParam.SetValue(lightDirection);
        }


	    /// <summary>
	    ///     sets the light direction using just an angle in degrees. 0 degrees points to theright, 90 degrees would be straight
	    ///     down, etc
	    /// </summary>
	    /// <param name="degrees">Degrees.</param>
	    public void SetSpotLightDirection(float degrees)
        {
            var radians = MathHelper.ToRadians(degrees);
            var dir = new Vector2((float) System.Math.Cos(radians), (float) System.Math.Sin(radians));
            SetSpotLightDirection(dir);
        }


        public void SetSpotConeAngle(float coneAngle)
        {
            _coneAngleParam.SetValue(coneAngle);
        }

        #endregion


        #region Directional light properties

        public void SetSpecularIntensity(float specIntensity)
        {
            _specularIntensityParam.SetValue(specIntensity);
        }


        public void SetSpecularPower(float specPower)
        {
            _specularPowerParam.SetValue(specPower);
        }


        public void SetAreaDirectionalLightDirection(Vector3 lightDir)
        {
            _dirAreaLightDirectionParam.SetValue(lightDir);
        }

        #endregion


        #region Final combine properties

        public void SetAmbientColor(Color color)
        {
            _ambientColorParam.SetValue(color.ToVector3());
        }


	    /// <summary>
	    ///     sets the two textures required for the final combine and applies the pass
	    /// </summary>
	    /// <param name="diffuse">Diffuse.</param>
	    /// <param name="lightMap">Light map.</param>
	    public void PrepareForFinalCombine(Texture2D diffuse, Texture2D lightMap, Texture2D normalMap)
        {
            _colorMapParam.SetValue(diffuse);
            _lightMapParam.SetValue(lightMap);
            _normalMapParam.SetValue(normalMap);

            _finalCombinePass.Apply();
        }

        #endregion
    }
}