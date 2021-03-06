﻿using Aurora.Framework.Modules;
using Aurora.Framework.Servers;
using Aurora.Framework.Servers.HttpServer;
using Aurora.Framework.Servers.HttpServer.Implementation;
using Aurora.Framework.Servers.HttpServer.Interfaces;
using Aurora.Framework.Services;
using Aurora.Framework.Services.ClassHelpers.Assets;
using Nini.Config;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aurora.Services
{
    public class AgentAppearanceService : IService, IAgentAppearanceService
    {
        public string ServiceURI { get; protected set; }
        protected IRegistryCore m_registry;
        protected IAssetService m_assetService;
        protected bool m_enabled = false;

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            IConfig ssaConfig = config.Configs["SSAService"];
            uint port = 8011;
            if (ssaConfig != null)
            {
                m_enabled = ssaConfig.GetBoolean("Enabled", m_enabled);
                port = ssaConfig.GetUInt("Port", port);
            }
            if (!m_enabled)
                return;
            IHttpServer server = registry.RequestModuleInterface<ISimulationBase>().GetHttpServer(port);
            ServiceURI = server.ServerURI + "/";
            server.AddHTTPHandler(new GenericStreamHandler("GET", "/texture/", GetBakedTexture));
            registry.RegisterModuleInterface<IAgentAppearanceService>(this);
            m_registry = registry;
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            if (!m_enabled) return;
            m_assetService = registry.RequestModuleInterface<IAssetService>();
        }

        public void FinishedStartup()
        {
            if (!m_enabled) return;
            IGridServerInfoService serverInfo = m_registry.RequestModuleInterface<IGridServerInfoService>();
            if (serverInfo != null)
                serverInfo.AddURI("SSAService", ServiceURI);
        }


        public byte[] GetBakedTexture(string path, Stream request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            string[] req = path.Split('/');
            UUID avID = UUID.Parse(req[2]);
            string type = req[3];
            UUID textureID = UUID.Parse(req[4]);

            //IAvatarService avService = m_registry.RequestModuleInterface<IAvatarService>();
            //Aurora.Framework.ClientInterfaces.AvatarAppearance appearance = avService.GetAppearance(avID);
            //AvatarTextureIndex textureIndex = AppearanceManager.BakeTypeToAgentTextureIndex((BakeType)Enum.Parse(typeof(BakeType), type, true));
            //AssetBase texture = m_assetService.Get(appearance.Texture.FaceTextures[(int)textureIndex].TextureID.ToString());
            AssetBase texture = m_assetService.Get(textureID.ToString());
            if (texture == null)
            {
                return new byte[0];
            }
            // Full content request
            httpResponse.StatusCode = (int)System.Net.HttpStatusCode.OK;
            httpResponse.ContentType = texture.TypeString;
            return texture.Data;
        }
    }
}
