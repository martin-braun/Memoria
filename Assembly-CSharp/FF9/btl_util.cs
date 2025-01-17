﻿using System;
using System.Collections.Generic;
using Memoria;
using Memoria.Data;
using Memoria.Scripts;
using UnityEngine;

namespace FF9
{
	public class btl_util
	{
		public static List<BTL_DATA> findAllBtlData(UInt16 id)
		{
			List<BTL_DATA> result = new List<BTL_DATA>();
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
				if ((next.btl_id & id) != 0)
					result.Add(next);
			return result;
		}

		public static PLAYER getPlayerPtr(BTL_DATA btl)
		{
			return FF9StateSystem.Common.FF9.player[(Int32)btl.bi.slot_no];
		}

		public static ENEMY getEnemyPtr(BTL_DATA btl)
		{
			return FF9StateSystem.Battle.FF9Battle.enemy[(Int32)btl.bi.slot_no];
		}

		public static ENEMY_TYPE getEnemyTypePtr(BTL_DATA btl)
		{
			return FF9StateSystem.Battle.FF9Battle.enemy[(Int32)btl.bi.slot_no].et;
		}

		public static Byte getWeaponNumber(BTL_DATA btl)
		{
			return FF9StateSystem.Common.FF9.player[(Int32)btl.bi.slot_no].equip[0];
		}

		public static Int32 btlItemNum(Int32 ff9item_no)
		{
			return ff9item_no - 224;
		}

		public static Int32 ff9WeaponNum(Int32 ff9item_no)
		{
			return ff9item_no;
		}

		public static Byte getSerialNumber(BTL_DATA btl)
		{
			if (btl.bi.player != 0)
				return FF9StateSystem.Common.FF9.player[(Int32)btl.bi.slot_no].info.serial_no;
			return 19;
		}

		public static CMD_DATA getCurCmdPtr()
		{
			return FF9StateSystem.Battle.FF9Battle.cur_cmd;
		}

		public static Boolean IsBtlUsingCommand(BTL_DATA btl, out CMD_DATA cmdUsed)
		{
			foreach (CMD_DATA cmd in FF9StateSystem.Battle.FF9Battle.cur_cmd_list)
				if (cmd.regist == btl)
				{
					cmdUsed = cmd;
					return true;
				}
			cmdUsed = null;
			return false;
		}

		public static Boolean IsBtlTargetOfCommand(BTL_DATA btl, List<CMD_DATA> cmdList = null)
		{
			foreach (CMD_DATA cmd in FF9StateSystem.Battle.FF9Battle.cur_cmd_list)
				if ((cmd.tar_id & btl.btl_id) != 0)
				{
					if (cmdList != null)
						cmdList.Add(cmd);
					else
						return true;
				}
			return cmdList != null && cmdList.Count > 0;
		}

		public static Boolean IsBtlUsingCommand(BTL_DATA btl)
		{
			return IsBtlUsingCommand(btl, out _);
		}

		public static Boolean IsBtlUsingCommandMotion(BTL_DATA btl, Boolean includeSysCmd = false)
		{
			foreach (CMD_DATA cmd in FF9StateSystem.Battle.FF9Battle.cur_cmd_list)
				if (cmd.regist == btl && cmd.info.cmd_motion && (includeSysCmd || cmd.cmd_no <= BattleCommandId.EnemyReaction || cmd.cmd_no >= BattleCommandId.ScriptCounter1))
					return true;
			return false;
		}

		public static Boolean IsBtlBusy(BTL_DATA btl, BusyMode mode)
		{
			if ((mode & BusyMode.ANY_CURRENT) != 0)
				foreach (CMD_DATA cmd in FF9StateSystem.Battle.FF9Battle.cur_cmd_list)
				{
					if ((mode & BusyMode.CASTER) != 0 && cmd.regist == btl)
						return true;
					if ((mode & BusyMode.TARGET) != 0 && ((cmd.tar_id & btl.btl_id) != 0 || (btl_cmd.MergeReflecTargetID(cmd.reflec) & btl.btl_id) != 0))
						return true;
					if ((mode & BusyMode.MAGIC_CASTER) != 0 && cmd.cmd_no == BattleCommandId.MagicSword && btl.bi.player != 0 && btl.bi.slot_no == 1)
						return true;
				}
			if ((mode & BusyMode.ANY_QUEUED) != 0)
				for (CMD_DATA cmd = FF9StateSystem.Battle.FF9Battle.cmd_queue; cmd != null; cmd = cmd.next)
				{
					if ((mode & BusyMode.QUEUED_CASTER) != 0 && cmd.regist == btl)
						return true;
					if ((mode & BusyMode.QUEUED_TARGET) != 0 && (cmd.tar_id & btl.btl_id) != 0)
						return true;
					if ((mode & BusyMode.QUEUED_MAGIC_CASTER) != 0 && cmd.cmd_no == BattleCommandId.MagicSword && btl.bi.player != 0 && btl.bi.slot_no == 1)
						return true;
				}
			return false;
		}

		public static BattleUnit GetMasterEnemyBtlPtr()
	    {
	        foreach (BattleUnit unit in FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits())
	            if (!unit.IsPlayer && !unit.IsSlave && unit.Enemy.Data.info.multiple != 0)
	                return unit;

	        return null;
	    }

	    public static UInt32 SumOfTarget(UInt32 player)
		{
			UInt32 count = 0u;
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
				if ((UInt32)next.bi.player == player && next.bi.target != 0 && !Status.checkCurStat(next, BattleStatus.Death))
					count++;
			return count;
		}

		public static UInt16 GetRandomBtlID(UInt32 player, Boolean allowDead = false)
		{
			UInt16[] array = new UInt16[4];
			UInt16 btlCount = 0;
			if (player != 0u)
				player = 1u;
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
				if ((UInt32)next.bi.player == player && (!Status.checkCurStat(next, BattleStatus.Death) || allowDead) && next.bi.target != 0)
					array[btlCount++] = next.btl_id;
			if (btlCount == 0)
				return 0;
			return array[Comn.random8() % (Int32)btlCount];
		}

		public static Boolean ManageBattleSong(FF9StateGlobal sys, UInt32 ticks, UInt32 song_id)
		{
			if ((sys.btl_flag & 16) == 0)
			{
				btlsnd.ff9btlsnd_song_vol_intplall((Int32)ticks, 0);
				sys.btl_flag = (Byte)(sys.btl_flag | 16);
			}
			if ((sys.btl_flag & 2) == 0)
			{
				FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
				if ((Int64)(ff9Battle.player_load_fade = (SByte)((Int32)ff9Battle.player_load_fade + 4)) < (Int64)((UInt64)ticks))
					return false;
				btlsnd.ff9btlsnd_song_load((Int32)song_id);
				sys.btl_flag = (Byte)(sys.btl_flag | 2);
			}
			if (btlsnd.ff9btlsnd_sync() != 0)
				return false;
			if ((sys.btl_flag & 32) == 0)
			{
				btlsnd.ff9btlsnd_song_play((Int32)song_id);
				sys.btl_flag = (Byte)(sys.btl_flag | 32);
			}
			return true;
		}

		public static UInt16 GetStatusBtlID(UInt32 list_no, BattleStatus status)
		{
			UInt16 num = 0;
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
			{
				if ((status == 0u || btl_stat.CheckStatus(next, status)) && next.bi.target != 0)
				{
					switch (list_no)
					{
					case 0u:
						if (next.bi.player != 0)
							num = (UInt16)(num | next.btl_id);
						break;
					case 1u:
						if (next.bi.player == 0)
							num = (UInt16)(num | next.btl_id);
						break;
					case 2u:
						num = (UInt16)(num | next.btl_id);
						break;
					}
				}
			}
			return num;
		}

		public static Boolean CheckEnemyCategory(BTL_DATA btl, Byte category)
		{
			return btl.bi.player == 0 && (btl_util.getEnemyTypePtr(btl).category & category) != 0;
		}

		public static UInt32 GetFF9CharNo(BTL_DATA btl)
		{
			return (UInt32)((btl.bi.player == 0) ? (9 + btl.bi.slot_no) : btl.bi.slot_no);
		}

		public static void SetEnemyDieSound(BTL_DATA btl, UInt16 snd_no)
		{
			FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
			if (ff9Battle.enemy_die == 0 && btl.bi.die_snd_f == 0)
			{
				ff9Battle.enemy_die = 8;
				btl_util.SetBattleSfx(btl, snd_no, 127);
				btl.bi.die_snd_f = 1;
			}
		}

		public static void SetBattleSfx(BTL_DATA btl, UInt16 snd_no, Byte volume)
		{
			if (snd_no != 65535)
			{
				UInt64 id = 0UL;
				btlsnd.ff9btlsnd_sndeffect_play((Int32)snd_no, 0, (Int32)volume, SeSnd.S_SeGetPos(id));
			}
		}

		public static void SetBBGColor(GameObject geo)
		{
			BBGINFO bbginfo = battlebg.nf_GetBbgInfoPtr();
			btl_util.GeoSetColor2Source(geo, bbginfo.chr_r, bbginfo.chr_g, bbginfo.chr_b);
		}

		public static void SetShadow(BTL_DATA btl, UInt16 x_radius, UInt32 z_radius)
		{
			UInt32 ff9CharNo = btl_util.GetFF9CharNo(btl);
			btl.shadow_x = (Byte)(x_radius * 16 / 224);
			btl.shadow_z = (Byte)(z_radius * 16u / 192u);
			btlshadow.FF9ShadowSetScaleBattle(ff9CharNo, btl.shadow_x, btl.shadow_z);
		}

		public static void SetFadeRate(BTL_DATA btl, Int32 rate)
		{
			if (rate >= 0 && rate < 32)
			{
				BBGINFO bbginfo = battlebg.nf_GetBbgInfoPtr();
				btl_util.GeoSetABR(btl.gameObject, "GEO_POLYFLAGS_TRANS_100_PLUS_25");
				btl_util.GeoSetColor2Source(btl.gameObject, (Byte)((Int32)bbginfo.chr_r * rate >> 5), (Byte)((Int32)bbginfo.chr_g * rate >> 5), (Byte)((Int32)bbginfo.chr_b * rate >> 5));
				if (btl.weapon_geo)
				{
					btl_util.GeoSetABR(btl.weapon_geo, "GEO_POLYFLAGS_TRANS_100_PLUS_25");
					btl_util.GeoSetColor2Source(btl.weapon_geo, (Byte)((Int32)bbginfo.chr_r * rate >> 5), (Byte)((Int32)bbginfo.chr_g * rate >> 5), (Byte)((Int32)bbginfo.chr_b * rate >> 5));
				}
			}
		}

		public static void SetEnemyFadeToPacket(BTL_DATA btl, Int32 rate)
		{
			BBGINFO bbginfo = battlebg.nf_GetBbgInfoPtr();
			btl_util.GeoSetABR(btl.gameObject, "GEO_POLYFLAGS_TRANS_100_PLUS_25");
			btl_util.GeoSetColor2DrawPacket(btl.gameObject, (Byte)((Int32)bbginfo.chr_r * rate >> 5), (Byte)((Int32)bbginfo.chr_g * rate >> 5), (Byte)((Int32)bbginfo.chr_b * rate >> 5), Byte.MaxValue);
			if (btl.bi.shadow != 0)
			{
				btl_util.GeoSetABR(btl.getShadow(), "GEO_POLYFLAGS_TRANS_100_PLUS_25");
				btl_util.GeoSetColor2DrawPacket(btl.getShadow(), (Byte)((Int32)bbginfo.chr_r * rate >> 5), (Byte)((Int32)bbginfo.chr_g * rate >> 5), (Byte)((Int32)bbginfo.chr_b * rate >> 5), Byte.MaxValue);
			}
			if (rate == 0)
				btl.SetDisappear(true, 5);
		}

		public static void GeoSetABR(GameObject go, String type)
		{
			Shader shader;
			if (type == "GEO_POLYFLAGS_TRANS_100_PLUS_25")
				shader = FF9StateSystem.Battle.fadeShader;
			else if (type == "SEMI_TRANS_50_PLUS_50" || type == "PSX/BattleMap_StatusEffect")
				shader = FF9StateSystem.Battle.battleShader;
			else if (type == "SHADOW" || type == "PSX/BattleMap_Abr_2")
				shader = FF9StateSystem.Battle.shadowShader;
			else
				shader = ShadersLoader.Find(type);
			SkinnedMeshRenderer[] componentsInChildren = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (Int32 i = 0; i < (Int32)componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.shader = shader;
				componentsInChildren[i].material.SetFloat("_Cutoff", 0.5f);
				componentsInChildren[i].material.SetTexture("_DetailTex", FF9StateSystem.Battle.detailTexture);
			}
			MeshRenderer[] componentsInChildren2 = go.GetComponentsInChildren<MeshRenderer>();
			for (Int32 j = 0; j < (Int32)componentsInChildren2.Length; j++)
			{
				Material[] materials = componentsInChildren2[j].materials;
				for (Int32 k = 0; k < (Int32)materials.Length; k++)
				{
					Material material = materials[k];
					material.shader = shader;
					material.SetFloat("_Cutoff", 0.5f);
					material.SetTexture("_DetailTex", FF9StateSystem.Battle.detailTexture);
				}
			}
		}

		public static void GeoSetColor2Source(GameObject go, Byte r, Byte g, Byte b)
		{
			btl_util.GeoSetColor2DrawPacket(go, r, g, b, Byte.MaxValue);
		}

		public static void GeoSetColor2DrawPacket(GameObject go, Byte r, Byte g, Byte b, Byte a = 255)
		{
			if (r > 255)
				r = Byte.MaxValue;
			if (g > 255)
				g = Byte.MaxValue;
			if (b > 255)
				b = Byte.MaxValue;
			SkinnedMeshRenderer[] componentsInChildren = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (Int32 i = 0; i < (Int32)componentsInChildren.Length; i++)
				componentsInChildren[i].material.SetColor("_Color", new Color32(r, g, b, a));
			MeshRenderer[] componentsInChildren2 = go.GetComponentsInChildren<MeshRenderer>();
			for (Int32 j = 0; j < (Int32)componentsInChildren2.Length; j++)
			{
				Material[] materials = componentsInChildren2[j].materials;
				for (Int32 k = 0; k < (Int32)materials.Length; k++)
				{
					Material material = materials[k];
					material.SetColor("_Color", new Color32(r, g, b, a));
				}
			}
		}

		[Flags]
		public enum BusyMode
		{
			CASTER = 1,
			TARGET = 2,
			MAGIC_CASTER = 4,
			QUEUED_CASTER = 8,
			QUEUED_TARGET = 16,
			QUEUED_MAGIC_CASTER = 32,
			ANY_CURRENT = CASTER | TARGET | MAGIC_CASTER,
			ANY_QUEUED = QUEUED_CASTER | QUEUED_TARGET | QUEUED_MAGIC_CASTER,
			ANY = ANY_CURRENT | ANY_QUEUED
		}
	}
}
