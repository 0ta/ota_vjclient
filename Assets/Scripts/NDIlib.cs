// NOTE : The following MIT license applies to this file ONLY and not to the SDK as a whole. Please review the SDK documentation 
// for the description of the full license terms, which are also provided in the file "NDI License Agreement.pdf" within the SDK or 
// online at http://new.tk/ndisdk_license/. Your use of any part of this SDK is acknowledgment that you agree to the SDK license 
// terms. The full NDI SDK may be downloaded at http://ndi.tv/
//
//*************************************************************************************************************************************
// 
// Copyright (C)2014-2021, NewTek, inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, 
// merge, publish, distribute, sublicense, and / or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace ota.ndi
{
	[SuppressUnmanagedCodeSecurity]
	public static partial class NDIlib
	{
		// An enumeration to specify the type of a packet returned by the functions
		public enum frame_type_e
		{
			frame_type_none = 0,
			frame_type_video = 1,
			frame_type_audio = 2,
			frame_type_metadata = 3,
			frame_type_error = 4,

			// This indicates that the settings on this input have changed.
			// For instamce, this value will be returned from NDIlib_recv_capture_v2 and NDIlib_recv_capture
			// when the device is known to have new settings, for instance the web-url has changed ot the device
			// is now known to be a PTZ camera.
			frame_type_status_change = 100
		}

		public enum FourCC_type_e
		{
			// YCbCr color space
			FourCC_type_UYVY = 0x59565955,

			// 4:2:0 formats
			NDIlib_FourCC_video_type_YV12 = 0x32315659,
			NDIlib_FourCC_video_type_NV12 = 0x3231564E,
			NDIlib_FourCC_video_type_I420 = 0x30323449,

			// BGRA
			FourCC_type_BGRA = 0x41524742,
			FourCC_type_BGRX = 0x58524742,

			// RGBA
			FourCC_type_RGBA = 0x41424752,
			FourCC_type_RGBX = 0x58424752,

			// This is a UYVY buffer followed immediately by an alpha channel buffer.
			// If the stride of the YCbCr component is "stride", then the alpha channel
			// starts at image_ptr + yres*stride. The alpha channel stride is stride/2.
			FourCC_type_UYVA = 0x41565955
		}

		public enum frame_format_type_e
		{
			// A progressive frame
			frame_format_type_progressive = 1,

			// A fielded frame with the field 0 being on the even lines and field 1 being
			// on the odd lines/
			frame_format_type_interleaved = 0,

			// Individual fields
			frame_format_type_field_0 = 2,
			frame_format_type_field_1 = 3
		}

		// FourCC values for audio frames
		public enum FourCC_audio_type_e
		{
			// Planar 32-bit floating point. Be sure to specify the channel stride.
			FourCC_audio_type_FLTP = 0x70544c46,
			FourCC_type_FLTP = FourCC_audio_type_FLTP,

			// Ensure that the size is 32bits
			FourCC_audio_type_max = 0x7fffffff
		}

		// This is a descriptor of a NDI source available on the network.
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct source_t
		{
			// A UTF8 string that provides a user readable name for this source.
			// This can be used for serialization, etc... and comprises the machine
			// name and the source name on that machine. In the form
			//		MACHINE_NAME (NDI_SOURCE_NAME)
			// If you specify this parameter either as NULL, or an EMPTY string then the
			// specific ip addres adn port number from below is used.
			public IntPtr p_ndi_name;

			// A UTF8 string that provides the actual network address and any parameters. 
			// This is not meant to be application readable and might well change in the future.
			// This can be nullptr if you do not know it and the API internally will instantiate
			// a finder that is used to discover it even if it is not yet available on the network.
			public IntPtr p_url_address;
		}

		// This describes a video frame
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct video_frame_v2_t
		{
			// The resolution of this frame
			public int xres, yres;

			// What FourCC this is with. This can be two values
			public FourCC_type_e FourCC;

			// What is the frame-rate of this frame.
			// For instance NTSC is 30000,1001 = 30000/1001 = 29.97fps
			public int frame_rate_N, frame_rate_D;

			// What is the picture aspect ratio of this frame.
			// For instance 16.0/9.0 = 1.778 is 16:9 video
			// 0 means square pixels
			public float picture_aspect_ratio;

			// Is this a fielded frame, or is it progressive
			public frame_format_type_e frame_format_type;

			// The timecode of this frame in 100ns intervals
			public Int64 timecode;

			// The video data itself
			public IntPtr p_data;

			// The inter line stride of the video data, in bytes.
			public int line_stride_in_bytes;

			// Per frame metadata for this frame. This is a NULL terminated UTF8 string that should be
			// in XML format. If you do not want any metadata then you may specify NULL here.
			public IntPtr p_metadata;

			// This is only valid when receiving a frame and is specified as a 100ns time that was the exact
			// moment that the frame was submitted by the sending side and is generated by the SDK. If this
			// value is NDIlib_recv_timestamp_undefined then this value is not available and is NDIlib_recv_timestamp_undefined.
			public Int64 timestamp;
		}

		// This describes an audio frame
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct audio_frame_v2_t
		{
			// The sample-rate of this buffer
			public int sample_rate;

			// The number of audio channels
			public int no_channels;

			// The number of audio samples per channel
			public int no_samples;

			// The timecode of this frame in 100ns intervals
			public Int64 timecode;

			// The audio data
			public IntPtr p_data;

			// The inter channel stride of the audio channels, in bytes
			public int channel_stride_in_bytes;

			// Per frame metadata for this frame. This is a NULL terminated UTF8 string that should be
			// in XML format. If you do not want any metadata then you may specify NULL here.
			public IntPtr p_metadata;

			// This is only valid when receiving a frame and is specified as a 100ns time that was the exact
			// moment that the frame was submitted by the sending side and is generated by the SDK. If this
			// value is NDIlib_recv_timestamp_undefined then this value is not available and is NDIlib_recv_timestamp_undefined.
			public Int64 timestamp;
		}

		// This describes an audio frame
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct audio_frame_v3_t
		{
			// The sample-rate of this buffer
			public int sample_rate;

			// The number of audio channels
			public int no_channels;

			// The number of audio samples per channel
			public int no_samples;

			// The timecode of this frame in 100ns intervals
			public Int64 timecode;

			// What FourCC describing the type of data for this frame
			FourCC_audio_type_e FourCC;

			// The audio data
			public IntPtr p_data;

			// If the FourCC is not a compressed type and the audio format is planar,
			// then this will be the stride in bytes for a single channel.
			// If the FourCC is a compressed type, then this will be the size of the
			// p_data buffer in bytes.
			public int channel_stride_in_bytes;

			// Per frame metadata for this frame. This is a NULL terminated UTF8 string that should be
			// in XML format. If you do not want any metadata then you may specify NULL here.
			public IntPtr p_metadata;

			// This is only valid when receiving a frame and is specified as a 100ns time that was the exact
			// moment that the frame was submitted by the sending side and is generated by the SDK. If this
			// value is NDIlib_recv_timestamp_undefined then this value is not available and is NDIlib_recv_timestamp_undefined.
			public Int64 timestamp;
		}

		// The data description for metadata
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct metadata_frame_t
		{
			// The length of the string in UTF8 characters. This includes the NULL terminating character.
			// If this is 0, then the length is assume to be the length of a NULL terminated string.
			public int length;

			// The timecode of this frame in 100ns intervals
			public Int64 timecode;

			// The metadata as a UTF8 XML string. This is a NULL terminated string.
			public IntPtr p_data;
		}

		// Tally structures
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct tally_t
		{
			// Is this currently on program output
			[MarshalAsAttribute(UnmanagedType.U1)]
			public bool on_program;

			// Is this currently on preview output
			[MarshalAsAttribute(UnmanagedType.U1)]
			public bool on_preview;
		}

		// When you specify this as a timecode, the timecode will be synthesized for you. This may
		// be used when sending video, audio or metadata. If you never specify a timecode at all,
		// asking for each to be synthesized, then this will use the current system time as the
		// starting timecode and then generate synthetic ones, keeping your streams exactly in
		// sync as long as the frames you are sending do not deviate from the system time in any
		// meaningful way. In practice this means that if you never specify timecodes that they
		// will always be generated for you correctly. Timecodes coming from different senders on
		// the same machine will always be in sync with eachother when working in this way. If you
		// have NTP installed on your local network, then streams can be synchronized between
		// multiple machines with very high precision.
		//
		// If you specify a timecode at a particular frame (audio or video), then ask for all subsequent
		// ones to be synthesized. The subsequent ones will be generated to continue this sequency
		// maintining the correct relationship both the between streams and samples generated, avoiding
		// them deviating in time from the timecode that you specified in any meanginfful way.
		//
		// If you specify timecodes on one stream (e.g. video) and ask for the other stream (audio) to
		// be sythesized, the correct timecodes will be generated for the other stream and will be synthesize
		// exactly to match (they are not quantized inter-streams) the correct sample positions.
		//
		// When you send metadata messagesa and ask for the timecode to be synthesized, then it is chosen
		// to match the closest audio or video frame timecode so that it looks close to something you might
		// want ... unless there is no sample that looks close in which a timecode is synthesized from the
		// last ones known and the time since it was sent.
		//
		public static Int64 send_timecode_synthesize = Int64.MaxValue;

		// If the time-stamp is not available (i.e. a version of a sender before v2.5)
		public static Int64 recv_timestamp_undefined = Int64.MaxValue;

		//
		//ota add
		//added find related DllImport related struct
		//
		// The creation structure that is used when you are creating a finder
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct find_create_t
		{
			// Do we want to incluide the list of NDI sources that are running
			// on the local machine ?
			// If TRUE then local sources will be visible, if FALSE then they
			// will not.
			[MarshalAsAttribute(UnmanagedType.U1)]
			public bool show_local_sources;

			// Which groups do you want to search in for sources
			public IntPtr p_groups;

			// The list of additional IP addresses that exist that we should query for
			// sources on. For instance, if you want to find the sources on a remote machine
			// that is not on your local sub-net then you can put a comma seperated list of
			// those IP addresses here and those sources will be available locally even though
			// they are not mDNS discoverable. An example might be "12.0.0.8,13.0.12.8".
			// When none is specified the registry is used.
			// Default = NULL;
			public IntPtr p_extra_ips;
		}

		//
		//ota add
		//added receive related DllImport related struct
		//
		public enum recv_bandwidth_e
		{
			// Receive metadata.
			recv_bandwidth_metadata_only = -10,

			// Receive metadata audio.
			recv_bandwidth_audio_only = 10,

			// Receive metadata audio video at a lower bandwidth and resolution.
			recv_bandwidth_lowest = 0,

			// Receive metadata audio video at full resolution.
			recv_bandwidth_highest = 100
		}

		public enum recv_color_format_e
		{
			// No alpha channel: BGRX Alpha channel: BGRA
			recv_color_format_BGRX_BGRA = 0,

			// No alpha channel: UYVY Alpha channel: BGRA
			recv_color_format_UYVY_BGRA = 1,

			// No alpha channel: RGBX Alpha channel: RGBA
			recv_color_format_RGBX_RGBA = 2,

			// No alpha channel: UYVY Alpha channel: RGBA
			recv_color_format_UYVY_RGBA = 3,

			// On Windows there are some APIs that require bottom to top images in RGBA format. Specifying
			// this format will return images in this format. The image data pointer will still point to the
			// "top" of the image, althought he stride will be negative. You can get the "bottom" line of the image
			// using : video_data.p_data + (video_data.yres - 1)*video_data.line_stride_in_bytes
			recv_color_format_BGRX_BGRA_flipped = 200,

			// Read the SDK documentation to understand the pros and cons of this format.
			recv_color_format_fastest = 100,

			// Legacy definitions for backwards compatibility
			recv_color_format_e_BGRX_BGRA = recv_color_format_BGRX_BGRA,
			recv_color_format_e_UYVY_BGRA = recv_color_format_UYVY_BGRA,
			recv_color_format_e_RGBX_RGBA = recv_color_format_RGBX_RGBA,
			recv_color_format_e_UYVY_RGBA = recv_color_format_UYVY_RGBA
		}

		// The creation structure that is used when you are creating a receiver
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct recv_create_v3_t
		{
			// The source that you wish to connect to.
			public source_t source_to_connect_to;

			// Your preference of color space. See above.
			public recv_color_format_e color_format;

			// The bandwidth setting that you wish to use for this video source. Bandwidth
			// controlled by changing both the compression level and the resolution of the source.
			// A good use for low bandwidth is working on WIFI connections.
			public recv_bandwidth_e bandwidth;

			// When this flag is FALSE, all video that you receive will be progressive. For sources
			// that provide fields, this is de-interlaced on the receiving side (because we cannot change
			// what the up-stream source was actually rendering. This is provided as a convenience to
			// down-stream sources that do not wish to understand fielded video. There is almost no
			// performance impact of using this function.
			[MarshalAsAttribute(UnmanagedType.U1)]
			public bool allow_video_fields;

			// The name of the NDI receiver to create. This is a NULL terminated UTF8 string and should be
			// the name of receive channel that you have. This is in many ways symettric with the name of
			// senders, so this might be "Channel 1" on your system.
			public IntPtr p_ndi_recv_name;
		}

		// Get the current queue depths
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct recv_queue_t
		{
			// The number of video frames
			public int video_frames;

			// The number of audio frames
			public int audio_frames;

			// The number of metadata frames
			public int metadata_frames;
		}

		// This allows you determine the current performance levels of the receiving to be able to detect whether frames have been dropped
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct recv_performance_t
		{
			// The number of video frames
			public Int64 video_frames;

			// The number of audio frames
			public Int64 audio_frames;

			// The number of metadata frames
			public Int64 metadata_frames;
		}

		//
		//ota add
		//added receive related DllImport related struct
		//
		// The creation structure that is used when you are creating a sender
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct send_create_t
		{
			// The name of the NDI source to create. This is a NULL terminated UTF8 string.
			public IntPtr p_ndi_name;

			// What groups should this source be part of. NULL means default.
			public IntPtr p_groups;

			// Do you want audio and video to "clock" themselves. When they are clocked then
			// by adding video frames, they will be rate limited to match the current frame-rate
			// that you are submitting at. The same is true for audio. In general if you are submitting
			// video and audio off a single thread then you should only clock one of them (video is
			// probably the better of the two to clock off). If you are submtiting audio and video
			// of separate threads then having both clocked can be useful.
			[MarshalAsAttribute(UnmanagedType.U1)]
			public bool clock_video, clock_audio;
		}

		//
		//ota add
		//added find related DllImport only for x64 architecure CPU
		//
		[DllImport(Config.DllName, EntryPoint = "NDIlib_initialize", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern bool Initialize();

		// find_create_v2 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_find_create_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr find_create_v2(ref find_create_t p_create_settings);

		// find_destroy 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_find_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void find_destroy_64(IntPtr p_instance);

		// find_get_current_sources 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_find_get_current_sources", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr find_get_current_sources(IntPtr p_instance, ref UInt32 p_no_sources);

		// find_wait_for_sources 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_find_wait_for_sources", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAsAttribute(UnmanagedType.U1)]
		internal static extern bool find_wait_for_sources(IntPtr p_instance, UInt32 timeout_in_ms);

		//
		//ota add
		//added receive related DllImport only for x64 architecure CPU
		//
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_create_v3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr recv_create_v3(ref recv_create_v3_t p_create_settings);

		// recv_destroy 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_destroy(IntPtr p_instance);

		// This function allows you to change the connection to another video source, you can also disconnect it by specifying a IntPtr.Zero here. 
		// This allows you to preserve a receiver without needing to recreate it.
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_connect", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_connect_64(IntPtr p_instance, IntPtr p_src);


		// recv_capture_v2 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_capture_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern frame_type_e recv_capture_v2(IntPtr p_instance, ref video_frame_v2_t p_video_data, ref audio_frame_v2_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms);

		// recv_capture_v3 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_capture_v3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern frame_type_e recv_capture_v3_64(IntPtr p_instance, ref video_frame_v2_t p_video_data, ref audio_frame_v3_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms);

		// recv_free_video_v2 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_free_video_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_free_video_v2(IntPtr p_instance, ref video_frame_v2_t p_video_data);

		// recv_free_audio_v2 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_free_audio_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_free_audio_v2(IntPtr p_instance, ref audio_frame_v2_t p_audio_data);

		// recv_free_audio_v3 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_free_audio_v3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_free_audio_v3_64(IntPtr p_instance, ref audio_frame_v3_t p_audio_data);

		// recv_free_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_free_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_free_metadata(IntPtr p_instance, ref metadata_frame_t p_metadata);

		// recv_free_string 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_free_string", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_free_string_64(IntPtr p_instance, IntPtr p_string);

		// recv_send_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_send_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAsAttribute(UnmanagedType.U1)]
		internal static extern bool recv_send_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);

		// recv_set_tally 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_set_tally", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAsAttribute(UnmanagedType.U1)]
		internal static extern bool recv_set_tally(IntPtr p_instance, ref tally_t p_tally);

		// recv_get_performance 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_get_performance", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_get_performance_64(IntPtr p_instance, ref recv_performance_t p_total, ref recv_performance_t p_dropped);

		// recv_get_queue 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_get_queue", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_get_queue_64(IntPtr p_instance, ref recv_queue_t p_total);

		// recv_clear_connection_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_clear_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_clear_connection_metadata_64(IntPtr p_instance);

		// recv_add_connection_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_add_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void recv_add_connection_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);

		// recv_get_no_connections 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_get_no_connections", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int recv_get_no_connections_64(IntPtr p_instance);

		// recv_get_web_control 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_get_web_control", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr recv_get_web_control_64(IntPtr p_instance);

		// recv_ptz_is_supported 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_is_supported", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAsAttribute(UnmanagedType.U1)]
		internal static extern bool recv_ptz_is_supported(IntPtr p_instance);

		// recv_recording_is_supported 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_recording_is_supported", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAsAttribute(UnmanagedType.U1)]
		internal static extern bool recv_recording_is_supported(IntPtr p_instance);

		//
		//ota add
		//added send related DllImport only for x64 architecure CPU
		//
		// send_create 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_create", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr send_create(ref send_create_t p_create_settings);

		// send_destroy 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_destroy(IntPtr p_instance);

		// send_send_video_v2 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_send_video_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_send_video_v2_64(IntPtr p_instance, ref video_frame_v2_t p_video_data);

		// send_send_video_async_v2 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_send_video_async_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_send_video_async_v2(IntPtr p_instance, ref video_frame_v2_t p_video_data);

		// send_send_audio_v2 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_send_audio_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_send_audio_v2_64(IntPtr p_instance, ref audio_frame_v2_t p_audio_data);

		// send_send_audio_v3 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_send_audio_v3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_send_audio_v3_64(IntPtr p_instance, ref audio_frame_v3_t p_audio_data);

		// send_send_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_send_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_send_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);

		// send_capture 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_capture", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern frame_type_e send_capture_64(IntPtr p_instance, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms);

		// send_free_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_free_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_free_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);

		// send_get_tally 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_get_tally", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAsAttribute(UnmanagedType.U1)]
		internal static extern bool send_get_tally_64(IntPtr p_instance, ref tally_t p_tally, UInt32 timeout_in_ms);

		// send_get_no_connections 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_get_no_connections", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int send_get_no_connections_64(IntPtr p_instance, UInt32 timeout_in_ms);

		// send_clear_connection_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_clear_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_clear_connection_metadata_64(IntPtr p_instance);

		// send_add_connection_metadata 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_add_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_add_connection_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);

		// send_set_failover 
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_set_failover", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void send_set_failover_64(IntPtr p_instance, ref source_t p_failover_source);

		// Retrieve the source information for the given sender instance.  This pointer is valid until NDIlib_send_destroy is called.
		[DllImport(Config.DllName, EntryPoint = "NDIlib_send_get_source_name", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr send_get_source_name_64(IntPtr p_instance);

	} // class NDIlib

} // namespace NewTek

