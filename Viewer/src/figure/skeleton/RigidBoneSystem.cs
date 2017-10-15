﻿using System;
using System.Collections.Generic;
using System.Linq;

public class RigidBoneSystem {
	private readonly BoneSystem source;
	private readonly RigidBone[] bones;
	private readonly Dictionary<string, RigidBone> bonesByName;

	public RigidBoneSystem(BoneSystem source) {
		this.source = source;

		bones = new RigidBone[source.Bones.Count];
		for (int boneIdx = 0; boneIdx < bones.Length; ++boneIdx) {
			var sourceBone = source.Bones[boneIdx];
			bones[boneIdx] = new RigidBone(source.Bones[boneIdx], sourceBone.Parent != null ? bones[sourceBone.Parent.Index] : null);
		}
				
		bonesByName = bones.ToDictionary(bone => bone.Source.Name, bone => bone);
	}
	
	public RigidBone[] Bones => bones;
	public RigidBone RootBone => bones[0];
	public Dictionary<string, RigidBone> BonesByName => bonesByName;

	public void Synchronize(ChannelOutputs outputs) {
		while (outputs.Parent != null) {
			outputs = outputs.Parent;
		}

		for (int boneIdx = 0; boneIdx < bones.Length; ++boneIdx) {
			RigidBone bone = bones[boneIdx];
			bone.Synchronize(outputs);
		}
	}

	public StagedSkinningTransform[] GetBoneTransforms(ChannelOutputs outputs) {
		while (outputs.Parent != null) {
			outputs = outputs.Parent;
		}

		StagedSkinningTransform[] boneTransforms = new StagedSkinningTransform[bones.Length];

		for (int boneIdx = 0; boneIdx < bones.Length; ++boneIdx) {
			RigidBone bone = bones[boneIdx];
			RigidBone parent = bone.Parent;
			StagedSkinningTransform parentTransform = parent != null ? boneTransforms[parent.Source.Index] : StagedSkinningTransform.Identity;
			boneTransforms[boneIdx] = bone.GetChainedTransform(outputs, parentTransform);
		}

		return boneTransforms;
	}
}
