﻿using System.Collections.Generic;
using System.Numerics;

namespace AssetGenerator.ModelGroups
{
    [ModelGroupAttribute]
    class Material_Alpha : ModelGroup
    { 
        public Material_Alpha()
        {
            modelGroupName = ModelGroupName.Material_Alpha;
            onlyBinaryProperties = false;
            noPrerequisite = false;
            Runtime.Image baseColorTexture = new Runtime.Image
            {
                Uri = texture_BaseColor
            };
            usedImages.Add(baseColorTexture);
            List<Vector4> vertexColors = new List<Vector4>()
            {
                new Vector4( 0.3f, 0.3f, 0.3f, 0.4f),
                new Vector4( 0.3f, 0.3f, 0.3f, 0.2f),
                new Vector4( 0.3f, 0.3f, 0.3f, 0.8f),
                new Vector4( 0.3f, 0.3f, 0.3f, 0.6f)
            };
            properties = new List<Property>
            {
                new Property(Propertyname.VertexColor_Vector4_Float, vertexColors, group:2),
                new Property(Propertyname.AlphaMode_Mask, glTFLoader.Schema.Material.AlphaModeEnum.MASK, group:1),
                new Property(Propertyname.AlphaMode_Blend, glTFLoader.Schema.Material.AlphaModeEnum.BLEND, group:1),
                new Property(Propertyname.AlphaCutoff, 0.7f),
                new Property(Propertyname.DoubleSided, true),
                new Property(Propertyname.BaseColorFactor, new Vector4(1.0f, 1.0f, 1.0f, 0.6f)),
                new Property(Propertyname.BaseColorTexture, baseColorTexture),
            };
            specialProperties = new List<Property>
            {
                new Property(Propertyname.BaseColorTexture, baseColorTexture),
                new Property(Propertyname.VertexColor_Vector4_Float, vertexColors, group:2),
            };
            specialCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.AlphaMode_Mask),
                properties.Find(e => e.name == Propertyname.AlphaCutoff)));
            specialCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.DoubleSided),
                properties.Find(e => e.name == Propertyname.AlphaCutoff),
                properties.Find(e => e.name == Propertyname.AlphaMode_Mask)));
            specialCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.DoubleSided),
                properties.Find(e => e.name == Propertyname.AlphaMode_Blend)));
            specialCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.AlphaMode_Mask),
                properties.Find(e => e.name == Propertyname.BaseColorFactor)));
            specialCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.BaseColorFactor),
                properties.Find(e => e.name == Propertyname.BaseColorTexture),
                properties.Find(e => e.name == Propertyname.AlphaMode_Blend),
                properties.Find(e => e.name == Propertyname.VertexColor_Vector4_Float)));
            removeCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.AlphaCutoff)));
            removeCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.BaseColorFactor)));
            removeCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.VertexColor_Vector4_Float)));
            removeCombos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.BaseColorTexture)));
        }

        override public List<List<Property>> ApplySpecialProperties(ModelGroup test, List<List<Property>> combos)
        {
            // BaseColorTexture is used everywhere except in the empty set
            var baseColorTexture = specialProperties.Find(e => e.name == Propertyname.BaseColorTexture);
            foreach (var y in combos)
            {
                // Checks if the property is already in that combo, or vertexcolor
                if ((y.Find(e => e.name == baseColorTexture.name)) == null &&
                    (y.Find(e => LogStringHelper.GenerateNameWithSpaces(e.name.ToString()) ==
                    LogStringHelper.GenerateNameWithSpaces(Propertyname.VertexColor_Vector4_Float.ToString()))) == null)
                {
                    // Skip the empty set
                    if (y.Count > 0)
                    {
                        y.Add(baseColorTexture);
                    }
                }
            }

            // Add AlphaMode_Blend/Alphamode_Mask + VertexColor combos to the bottom, so BaseColorTexture isn't split up
            combos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.AlphaMode_Blend),
                properties.Find(e => e.name == Propertyname.VertexColor_Vector4_Float)));
            combos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.AlphaMode_Mask),
                properties.Find(e => e.name == Propertyname.VertexColor_Vector4_Float)));

            // Add two combos to the bottom, so they don't have a base color texture
            combos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.AlphaMode_Blend),
                properties.Find(e => e.name == Propertyname.BaseColorFactor)));
            combos.Add(ComboHelper.CustomComboCreation(
                properties.Find(e => e.name == Propertyname.AlphaMode_Mask),
                properties.Find(e => e.name == Propertyname.BaseColorFactor)));

            return combos;
        }

        public Runtime.GLTF SetModelAttributes(Runtime.GLTF wrapper, Runtime.Material material, List<Property> combo, ref glTFLoader.Schema.Gltf gltf)
        {

            foreach (Property property in combo)
            {
                if (property.name == Propertyname.AlphaMode_Opaque ||
                    property.name == Propertyname.AlphaMode_Mask ||
                    property.name == Propertyname.AlphaMode_Blend)
                {
                    material.AlphaMode = property.value;
                }
                else if (property.name == Propertyname.AlphaCutoff)
                {
                    material.AlphaCutoff = property.value;
                }
                else if (property.name == Propertyname.DoubleSided)
                {
                    material.DoubleSided = property.value;
                }
                else if (property.name == Propertyname.BaseColorFactor)
                {
                    if (material.MetallicRoughnessMaterial == null)
                    {
                        material.MetallicRoughnessMaterial = new Runtime.PbrMetallicRoughness();
                    }
                    material.MetallicRoughnessMaterial.BaseColorFactor = property.value;
                }
                else if (property.name == Propertyname.BaseColorTexture)
                {
                    if (material.MetallicRoughnessMaterial == null)
                    {
                        material.MetallicRoughnessMaterial = new Runtime.PbrMetallicRoughness();
                    }
                    material.MetallicRoughnessMaterial.BaseColorTexture = new Runtime.Texture();
                    material.MetallicRoughnessMaterial.BaseColorTexture.Source = property.value;
                }
                else if (property.name == Propertyname.VertexColor_Vector4_Float)
                {
                    wrapper.Scenes[0].Nodes[0].Mesh.MeshPrimitives[0].ColorComponentType = Runtime.MeshPrimitive.ColorComponentTypeEnum.FLOAT;
                    wrapper.Scenes[0].Nodes[0].Mesh.MeshPrimitives[0].ColorType = Runtime.MeshPrimitive.ColorTypeEnum.VEC4;
                    wrapper.Scenes[0].Nodes[0].Mesh.MeshPrimitives[0].Colors = property.value;
                }
            }
            wrapper.Scenes[0].Nodes[0].Mesh.MeshPrimitives[0].Material = material;

            return wrapper;
        }
    }
}