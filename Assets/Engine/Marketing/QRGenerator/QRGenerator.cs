// QRGenerator.cs
// 
// Created by Alger Ortín Castellví on 02/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using QRCoder;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple UI widget to show the progress of an addressable download.
/// </summary>
public class QRGenerator {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Defines how much percentage of the QR code can be lost before the code is not readable.
	/// The lesser the tolerance, the fastest the code is generated.
	/// </summary>
	public enum ErrorTolerance {
		PERCENT_7,
		PERCENT_15,
		PERCENT_25,
		PERCENT_30
	};

	public enum LogoFilterMode {
		NEAREST_NEIGHBOUR,
		BILINEAR,
		AVERAGE
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private static bool s_debugEnabled = false;
	public static bool DEBUG_ENABLED {
		get {
#if DEBUG 
			return s_debugEnabled;
#else
			return false;
#endif
		}
		set { s_debugEnabled = value; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generate a QR code image from a given text.
	/// </summary>
	/// <returns>A texture with the generated QR code representing the given text.</returns>
	/// <param name="_text">Text to be represented.</param>
	/// <param name="_size">Size ot the texture, in pixels. The bigger the size, the longer it takes to generate.</param>
	/// <param name="_foregroundColor">Foreground color.</param>
	/// <param name="_backgroundColor">Background color.</param>
	/// <param name="_errorTolerance">How much percentage of the QR code can be lost before the code is not readable. The lesser, the fastest the code is generated.</param>
	public static Texture2D GenerateQR(string _text, int _size, Color _foregroundColor, Color _backgroundColor, ErrorTolerance _errorTolerance = ErrorTolerance.PERCENT_25) {
		return GenerateQR(_text, _size, _foregroundColor, _backgroundColor, _errorTolerance, null, 0.25f, LogoFilterMode.BILINEAR);
	}

	/// <summary>
	/// Generate a QR code image from a given text.
	/// </summary>
	/// <returns>A texture with the generated QR code representing the given text.</returns>
	/// <param name="_text">Text to be represented.</param>
	/// <param name="_size">Size ot the texture, in pixels. The bigger the size, the longer it takes to generate.</param>
	/// <param name="_foregroundColor">Foreground color.</param>
	/// <param name="_backgroundColor">Background color.</param>
	/// <param name="_errorTolerance">How much percentage of the QR code can be lost before the code is not readable. The lesser, the fastest the code is generated.</param>
	/// <param name="_logo">Logo to be printed on top of the QR code. <c>null</c> for none. The texture must be <b>uncompressed</b>, <b>readable</b> and <b>not have mip-maps</b>.</param>
	/// <param name="_logoSize">Size of the logo in percentage of the <paramref name="_size"/>. Will be capped if it exceeds the <paramref name="_errorTolerance"/>.</param>
	/// <param name="_logoFilterMode">When re-scaling the logo, which algorithm to use. See http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/ .</param>
	public static Texture2D GenerateQR(string _text, int _size, Color _foregroundColor, Color _backgroundColor, ErrorTolerance _errorTolerance, Texture2D _logo, float _logoSize, LogoFilterMode _logoFilterMode = LogoFilterMode.BILINEAR) {
		// Transform some parameters to library format
		QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.H;
		switch(_errorTolerance) {
			case ErrorTolerance.PERCENT_7:	eccLevel = QRCodeGenerator.ECCLevel.L; break;
			case ErrorTolerance.PERCENT_15: eccLevel = QRCodeGenerator.ECCLevel.M; break;
			case ErrorTolerance.PERCENT_25: eccLevel = QRCodeGenerator.ECCLevel.Q; break;
			case ErrorTolerance.PERCENT_30: eccLevel = QRCodeGenerator.ECCLevel.H; break;
		}

		// Generate the QR
		QRCodeGenerator generator = new QRCodeGenerator();
		QRCodeData qrData = generator.CreateQrCode(
			_text, 
			eccLevel,
			false, 
			false, 
			QRCodeGenerator.EciMode.Default, 
			-1
		);

		// Generate the image
		// Aux vars
		Color32 foreground32 = _foregroundColor.ToColor32();
		Color32 background32 = _backgroundColor.ToColor32();
		int srcSize = qrData.ModuleMatrix.Count;
		int tgtSize = Mathf.Max(_size, srcSize);	// [AOC[ Make sure we have enough pixels in the target texture!
		int pixelsPerModule = tgtSize / srcSize;    // [AOC] Might not be exact, we will fill the rest of the texture with the background color

		if(DEBUG_ENABLED) {
			Debug.Log("srcSize = " + srcSize);
			Debug.Log("tgtSize = " + tgtSize);
			Debug.Log("pixelsPerModule = " + pixelsPerModule);
		}

		// Compute margins with remaining pixels
		int remainingPixels = tgtSize - srcSize * pixelsPerModule;
		RectInt margins = new RectInt(
			remainingPixels / 2,	// Left
			remainingPixels / 2,	// Bottom
			tgtSize - remainingPixels,	// Width
			tgtSize - remainingPixels	// Height
		);

		if(DEBUG_ENABLED) {
			Debug.Log("remainingPixels = " + remainingPixels);
			Debug.Log("margins = " + margins.xMin + ", " + margins.yMin + ", " + margins.xMax + ", " + margins.yMax);
		}

		// Create new texture
		Texture2D qrTex = new Texture2D(tgtSize, tgtSize);
		qrTex.GetRawTextureData();
		Color32[] qrTexData = qrTex.GetPixels32();

		// Populate the texture
		// Declare aux vars outside the loop
		bool fill = false;
		Color32 c = background32;
		int x, y, x2, y2, i, j = 0;
		string debugStr = "";

		// Fill margins
		c = background32;

		// Left
		if(DEBUG_ENABLED) c = Color.red.ToColor32();
		for(x2 = 0; x2 < margins.xMin; ++x2) {
			for(y2 = 0; y2 < tgtSize; ++y2) {
				qrTexData[y2 * tgtSize + x2] = c;
			}
		}

		// Right
		if(DEBUG_ENABLED) c = Color.green.ToColor32();
		for(x2 = margins.xMax; x2 < tgtSize; ++x2) {
			for(y2 = 0; y2 < tgtSize; ++y2) {
				qrTexData[y2 * tgtSize + x2] = c;
			}
		}

		// Bottom
		if(DEBUG_ENABLED) c = Color.blue.ToColor32();
		for(x2 = 0; x2 < tgtSize; ++x2) {
			for(y2 = 0; y2 < margins.yMin; ++y2) {
				qrTexData[y2 * tgtSize + x2] = c;
			}
		}

		// Top
		if(DEBUG_ENABLED) c = Color.yellow.ToColor32(); 
		for(x2 = 0; x2 < tgtSize; ++x2) {
			for(y2 = margins.yMax; y2 < tgtSize; ++y2) {
				qrTexData[y2 * tgtSize + x2] = c;
			}
		}

		// Iterate source texture
		for(x = 0; x < srcSize; ++x) {
			for(y = 0; y < srcSize; ++y) {
				// Is this pixel filled?
				fill = qrData.ModuleMatrix[y][x];

				// Choose color
				c = fill ? foreground32 : background32;
				//if(DEBUG_MODE) debugStr = "[" + x + "," + y + "]\n";

				// Set pixels on the target texture
				x2 = margins.xMin + x * pixelsPerModule;
				y2 = margins.yMin + (srcSize - y - 1) * pixelsPerModule;        // Because texture is filled bottom to top, reverse the Y order
				for(i = 0; i < pixelsPerModule; ++i) {
					for(j = 0; j < pixelsPerModule; ++j) {
						// Set color!
						qrTexData[(y2 + j) * tgtSize + (x2 + i)] = c;
						//if(DEBUG_MODE) debugStr += "\t[" + (x2 + i) + "," + (y2 + j) + "] = " + fill + "\n";
					}
				}
				//if(DEBUG_MODE) Debug.Log(debugStr);
			}
		}

		// Add logo
		if(_logo != null) {
			// Some aux vars
			Vector2 sourceLogoSize = new Vector2(_logo.width, _logo.height);

			// Find out final logo size adapted to our QR code texture
			// Keep aspect ratio (if logo is not squared)
			float logoAR = (float)_logo.width / (float)_logo.height;
			float logoMaxSize = (tgtSize - remainingPixels) * _logoSize;  // Percentage of the actual QR code area (exclude margins)
			Vector2 newLogoSize = new Vector2(
				logoAR > 1 ? logoMaxSize : logoMaxSize * logoAR,
				logoAR > 1 ? logoMaxSize / logoAR : logoMaxSize
			);
			if(DEBUG_ENABLED) {
				Debug.Log("ar: " + logoAR);
				Debug.Log("logoMAxSize: " + logoMaxSize);
			}

			// Compute logo bounds within the QR texture
			int tgtCenter = tgtSize / 2;
			Rect logoBounds = new Rect(
				tgtCenter - newLogoSize.x / 2,   // Left
				tgtCenter - newLogoSize.y / 2,   // Bottom
				newLogoSize.x,
				newLogoSize.y
			);
			if(DEBUG_ENABLED) Debug.Log("logo bounds: " + logoBounds);

			// Copy original logo content to the qr texture with the new size
			// Using resizing algorithms from http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/
			Vector2 pixelSize = new Vector2(
				sourceLogoSize.x / newLogoSize.x,
				sourceLogoSize.y / newLogoSize.y
			);

			Vector2 center = new Vector2();
			int xFrom, xTo, yFrom, yTo, samples = 0;
			Color colorTmp = new Color();
			Color32 finalColor = new Color32();
			Color32[] logoTextData = _logo.GetPixels32();
			for(x2 = (int)logoBounds.xMin; x2 < (int)logoBounds.xMax; ++x2) {
				for(y2 = (int)logoBounds.yMin; y2 < (int)logoBounds.yMax; ++y2) {
					// Figure out matching position in original texture
					center.x = Mathf.InverseLerp(logoBounds.xMin, logoBounds.xMax, x2) * (sourceLogoSize.x - 1);
					center.y = Mathf.InverseLerp(logoBounds.yMin, logoBounds.yMax, y2) * (sourceLogoSize.y - 1);

					// Find out matching color in the original texture
					// Depends on filtering algorithm
					switch(_logoFilterMode) {
						case LogoFilterMode.NEAREST_NEIGHBOUR: {
							// Compute color - NEARES NEIGHBOUR
							center.x = Mathf.Round(center.x);
							center.y = Mathf.Round(center.y);

							// Calculate source index
							i = (int)((center.y * sourceLogoSize.x) + center.x);

							// Copy Pixel
							finalColor = logoTextData[i];
						} break;

						case LogoFilterMode.BILINEAR: {
							// Get Ratios
							float xRatio = center.x - Mathf.Floor(center.x);
							float yRatio = center.y - Mathf.Floor(center.y);

							// Get Pixel index's
							int iTL = Mathf.Clamp(
								(int)((Mathf.Floor(center.y) * sourceLogoSize.x) + Mathf.Floor(center.x)),
								0, logoTextData.Length - 1
							);
							int iTR = Mathf.Clamp(
								(int)((Mathf.Floor(center.y) * sourceLogoSize.x) + Mathf.Ceil(center.x)),
								0, logoTextData.Length - 1
							);
							int iBL = Mathf.Clamp(
								(int)((Mathf.Ceil(center.y) * sourceLogoSize.x) + Mathf.Floor(center.x)),
								0, logoTextData.Length - 1
							);
							int iBR = Mathf.Clamp(
								(int)((Mathf.Ceil(center.y) * sourceLogoSize.x) + Mathf.Ceil(center.x)),
								0, logoTextData.Length - 1
							);

							// Calculate Color
							finalColor = Color32.Lerp(
								Color32.Lerp(logoTextData[iTL], logoTextData[iTR], xRatio),
								Color32.Lerp(logoTextData[iBL], logoTextData[iBR], xRatio),
								yRatio
							);
						} break;

						case LogoFilterMode.AVERAGE: {
							// Calculate grid around point in the original texture
							xFrom = (int)Mathf.Max(Mathf.Floor(center.x - (pixelSize.x * 0.5f)), 0);
							xTo = (int)Mathf.Min(Mathf.Ceil(center.x + (pixelSize.x * 0.5f)), sourceLogoSize.x - 1);
							yFrom = (int)Mathf.Max(Mathf.Floor(center.y - (pixelSize.y * 0.5f)), 0);
							yTo = (int)Mathf.Min(Mathf.Ceil(center.y + (pixelSize.y * 0.5f)), sourceLogoSize.y - 1);

							// Loop through grid and accumulate
							samples = 0;
							colorTmp = Colors.transparentBlack;
							for(int ix = xFrom; ix < xTo; ix++) {
								for(int iy = yFrom; iy < yTo; iy++) {
									// Compute index in the logo texture
									i = iy * (int)sourceLogoSize.x + ix;
									
									// Add color to later compute the average
									colorTmp += logoTextData[i].ToColor();
									samples++;
								}
							}

							// Compute average color
							finalColor = (colorTmp / (float)samples).ToColor32();
						} break;
					}

					// Store new color in the target texture, blending with the original color using logo's alpha
					if(DEBUG_ENABLED) {
						debugStr = qrTexData[y2 * tgtSize + x2] + " vs " + finalColor + " (" + (finalColor.a / 255f) + ") -> ";
					}
					qrTexData[y2 * tgtSize + x2] = Color32.Lerp(
						qrTexData[y2 * tgtSize + x2],
						finalColor,
						finalColor.a / 255f
					);
					if(DEBUG_ENABLED) {
						finalColor = qrTexData[y2 * tgtSize + x2];
						debugStr += finalColor;
						Debug.Log(finalColor.ToColor().Tag(debugStr));
					}
				}
			}
		}

		// Upload to the GPU
		qrTex.SetPixels32(qrTexData);
		qrTex.Apply();

		// Done!
		return qrTex;
	}
}